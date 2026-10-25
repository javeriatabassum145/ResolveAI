using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ResolveAI.Api.Data;
using ResolveAI.Api.DTOs;
using ResolveAI.Api.Models;

namespace ResolveAI.Api.Services;

public class AIInvestigationService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AIInvestigationService(
        AppDbContext context,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _context = context;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<AIInvestigationResult> InvestigateAsync(
        int paymentId,
        string question)
    {
        // 1. Load payment and related data
        var payment = await _context.Payments
            .Include(p => p.Refunds)
            .Include(p => p.Events)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            throw new KeyNotFoundException(
                $"Payment with ID {paymentId} was not found.");
        }

        // 2. Build investigation context
        var investigationContext =
            await BuildInvestigationContextAsync(payment);

        // 3. Ask Gemini
        var aiResult = await AskGeminiAsync(
            question,
            investigationContext);

        // 4. Save investigation result
        var investigation = new Investigation
        {
            PaymentId = payment.Id,
            Question = question,
            Status = aiResult.RiskLevel == "LOW"
                ? "NoAnomaly"
                : "AnomalyDetected",

            Summary = aiResult.Summary,
            RootCause = aiResult.RootCause,
            Recommendation = aiResult.Recommendation,
            Confidence = aiResult.Confidence,
            RiskScore = aiResult.RiskScore,
            RiskLevel = aiResult.RiskLevel
        };

        _context.Investigations.Add(investigation);

        await _context.SaveChangesAsync();

        return aiResult;
    }

    // ---------------------------------------------------------
    // BUILD EVIDENCE CONTEXT
    // ---------------------------------------------------------

    private async Task<string> BuildInvestigationContextAsync(
        Payment payment)
    {
        var totalRefunded = payment.Refunds
            .Sum(r => r.Amount);

        var unexplainedDifference =
            payment.Amount - totalRefunded;

        var builder = new StringBuilder();

        builder.AppendLine("PAYMENT FORENSICS DATA");
        builder.AppendLine("======================");
        builder.AppendLine();

        builder.AppendLine($"Payment ID: {payment.Id}");
        builder.AppendLine($"PayPal Order ID: {payment.PayPalOrderId}");
        builder.AppendLine($"Amount: {payment.Amount} {payment.Currency}");
        builder.AppendLine($"Status: {payment.Status}");
        builder.AppendLine($"Created At: {payment.CreatedAt:O}");
        builder.AppendLine();

        builder.AppendLine("REFUND SUMMARY");
        builder.AppendLine("--------------");
        builder.AppendLine($"Refund Count: {payment.Refunds.Count}");
        builder.AppendLine(
            $"Total Refunded: {totalRefunded} {payment.Currency}");
        builder.AppendLine(
            $"Unexplained Difference: {unexplainedDifference} {payment.Currency}");
        builder.AppendLine();

        if (payment.Refunds.Any())
        {
            builder.AppendLine("REFUNDS");
            builder.AppendLine("-------");

            foreach (var refund in payment.Refunds)
            {
                builder.AppendLine(
                    $"Refund ID: {refund.PayPalRefundId}");

                builder.AppendLine(
                    $"Amount: {refund.Amount} {refund.Currency}");

                builder.AppendLine(
                    $"Status: {refund.Status}");

                builder.AppendLine(
                    $"Created At: {refund.CreatedAt:O}");

                builder.AppendLine();
            }
        }
        else
        {
            builder.AppendLine("REFUNDS");
            builder.AppendLine("-------");
            builder.AppendLine("No refunds found.");
            builder.AppendLine();
        }

        if (payment.Events.Any())
        {
            builder.AppendLine("PAYMENT EVENTS");
            builder.AppendLine("--------------");

            foreach (var paymentEvent in payment.Events)
            {
                builder.AppendLine(
                    $"Event Type: {paymentEvent.EventType}");

                builder.AppendLine(
                    $"Description: {paymentEvent.Description}");

                builder.AppendLine(
                    $"Occurred At: {paymentEvent.OccurredAt:O}");

                builder.AppendLine();
            }
        }
        else
        {
            builder.AppendLine("PAYMENT EVENTS");
            builder.AppendLine("--------------");
            builder.AppendLine("No payment events found.");
            builder.AppendLine();
        }

        return await Task.FromResult(builder.ToString());
    }

    // ---------------------------------------------------------
    // GEMINI
    // ---------------------------------------------------------

    private async Task<AIInvestigationResult> AskGeminiAsync(
        string question,
        string investigationContext)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = _configuration["Gemini:Model"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is missing.");
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            model = "gemini-2.5-flash";
        }

        var prompt =
            "You are ResolveAI, an evidence-based AI payment forensics investigator.\n\n" +

            "OBJECTIVE:\n" +
            "Investigate payment discrepancies using the supplied payment, refund, " +
            "and event records. Produce an accurate, explainable forensic report.\n\n" +

            "STRICT RULES:\n" +
            "1. Use only the supplied evidence. Never invent transactions or events.\n" +
            "2. A partial refund is not automatically suspicious or fraudulent.\n" +
            "3. A remaining balance is not automatically missing money.\n" +
            "4. Distinguish verified facts from possible explanations.\n" +
            "5. Do not claim fraud, unauthorized access, or ledger manipulation " +
            "unless evidence directly supports it.\n" +
            "6. Consider refund count, refund status, payment status, and event history.\n" +
            "7. If evidence is insufficient, explicitly say so and recommend checks.\n" +
            "8. Risk scores must be consistent with the evidence and explanation.\n\n" +

            "RISK SCORING GUIDELINES:\n" +
            "LOW (0-24): No meaningful anomaly is supported by the available evidence.\n" +
            "MEDIUM (25-49): A discrepancy or incomplete explanation needs review.\n" +
            "HIGH (50-79): Multiple or significant indicators warrant investigation.\n" +
            "CRITICAL (80-100): Strong evidence of a serious transaction anomaly exists.\n\n" +

            "IMPORTANT SCORING RULE:\n" +
            "A partial refund alone should normally receive LOW or MEDIUM risk, " +
            "unless additional evidence indicates a serious anomaly. " +
            "Do not assign HIGH or CRITICAL solely because the payment amount " +
            "is greater than the refund amount.\n\n" +

            "OUTPUT REQUIREMENTS:\n" +
            "Return only valid JSON. Do not include Markdown or code fences.\n" +
            "RiskLevel must be LOW, MEDIUM, HIGH, or CRITICAL.\n" +
            "RiskScore must be an integer from 0 to 100 and match the risk level.\n" +
            "Confidence must be a decimal between 0 and 1.\n" +
            "Evidence must contain specific facts from the supplied records.\n" +
            "Recommendation must suggest practical verification steps.\n\n" +

            "Return exactly this JSON structure:\n" +
            "{\n" +
            "  \"riskLevel\": \"MEDIUM\",\n" +
            "  \"riskScore\": 35,\n" +
            "  \"summary\": \"Brief explanation of the findings\",\n" +
            "  \"rootCause\": \"Known cause or explicitly stated possible explanation\",\n" +
            "  \"evidence\": [\"Verified fact 1\", \"Verified fact 2\"],\n" +
            "  \"recommendation\": \"Practical next step\",\n" +
            "  \"confidence\": 0.90\n" +
            "}\n\n" +

            "USER QUESTION:\n" + question + "\n\n" +
            "PAYMENT EVIDENCE:\n" + investigationContext;

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json"
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}");

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Gemini API error: {response.StatusCode} - {responseBody}");
        }

        using var document =
            JsonDocument.Parse(responseBody);

        var generatedText =
            document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

        if (string.IsNullOrWhiteSpace(generatedText))
        {
            throw new Exception(
                "Gemini returned an empty response.");
        }

        generatedText = generatedText.Trim();

        // Remove markdown code fences if Gemini returns them.
        if (generatedText.StartsWith("```"))
        {
            generatedText = generatedText
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();
        }

        var result =
            JsonSerializer.Deserialize<AIInvestigationResult>(
                generatedText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (result == null)
        {
            throw new InvalidOperationException(
                "Unable to parse AI investigation result.");
        }

        return result;
    }
}