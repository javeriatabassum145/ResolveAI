
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResolveAI.Api.Data;
using ResolveAI.Api.Services;

namespace ResolveAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvestigationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly AIInvestigationService _aiService;

    public InvestigationsController(
        AppDbContext context,
        AIInvestigationService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    // POST /api/Investigations/4
    [HttpPost("{paymentId:int}")]
    public async Task<IActionResult> Investigate(
        int paymentId,
        [FromBody] InvestigationRequest? request)
    {
        var paymentExists = await _context.Payments
            .AnyAsync(p => p.Id == paymentId);

        if (!paymentExists)
        {
            return NotFound(new
            {
                message = "Payment not found."
            });
        }

        var question = string.IsNullOrWhiteSpace(request?.Question)
            ? "Investigate this payment for anomalies and refund discrepancies."
            : request.Question.Trim();

        try
        {
            var result = await _aiService.InvestigateAsync(
                paymentId,
                question);

            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log the actual error in the backend terminal.
            Console.Error.WriteLine(ex);

            return StatusCode(500, new
            {
                message = "AI investigation failed. Check the backend logs."
            });
        }
    }

    // GET /api/Investigations
    [HttpGet]
    public async Task<IActionResult> GetInvestigations()
    {
        var investigations = await _context.Investigations
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new
            {
                i.Id,
                i.PaymentId,
                i.Question,
                i.Status,
                i.Summary,
                i.RootCause,
                i.Recommendation,
                i.Confidence,
                i.RiskLevel,
                i.RiskScore,
                i.CreatedAt
            })
            .ToListAsync();

        return Ok(investigations);
    }
}

public class InvestigationRequest
{
    public string? Question { get; set; }
}
