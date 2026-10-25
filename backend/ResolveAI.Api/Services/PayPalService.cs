using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ResolveAI.Api.Services;

public class PayPalService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public PayPalService(
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    private string BaseUrl =>
        _configuration["PayPal:BaseUrl"]
        ?? "https://api-m.sandbox.paypal.com";

    public async Task<string> GetAccessTokenAsync()
    {
        var clientId = _configuration["PayPal:ClientId"];
        var secret = _configuration["PayPal:Secret"];

        if (string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException(
                "PayPal ClientId or Secret is missing.");
        }

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{clientId}:{secret}")
        );

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{BaseUrl}/v1/oauth2/token");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        request.Content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            });

        var response = await _httpClient.SendAsync(request);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"PayPal authentication failed: {responseContent}");
        }

        using var json =
            JsonDocument.Parse(responseContent);

        var token =
            json.RootElement
                .GetProperty("access_token")
                .GetString();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "PayPal access token was not returned.");
        }

        return token;
    }

    public async Task<JsonElement> GetOrderAsync(string orderId)
    {
        var token = await GetAccessTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{BaseUrl}/v2/checkout/orders/{orderId}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var response =
            await _httpClient.SendAsync(request);

        var content =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"PayPal order request failed: {content}");
        }

        using var document =
            JsonDocument.Parse(content);

        return document.RootElement.Clone();
    }


    public async Task<JsonElement> CreateOrderAsync(
        decimal amount,
        string currency = "USD")
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount), "Amount must be greater than zero.");
        }

        var token = await GetAccessTokenAsync();

        var payload = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
            new
            {
                amount = new
                {
                    currency_code = currency,
                    value = amount.ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture)
                }
            }
        }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{BaseUrl}/v2/checkout/orders");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"PayPal order creation failed: {content}");
        }

        using var document = JsonDocument.Parse(content);
        return document.RootElement.Clone();
    }


    public async Task<JsonElement> CaptureOrderAsync(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new ArgumentException("Order ID is required.", nameof(orderId));
        }

        var token = await GetAccessTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{BaseUrl}/v2/checkout/orders/{Uri.EscapeDataString(orderId)}/capture");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        request.Content = new StringContent(
            "{}",
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"PayPal capture failed ({(int)response.StatusCode}): {content}");
        }

        using var document = JsonDocument.Parse(content);
        return document.RootElement.Clone();
    }


    public async Task<JsonElement> GetCaptureAsync(string captureId)
    {
        if (string.IsNullOrWhiteSpace(captureId))
        {
            throw new ArgumentException(
                "Capture ID is required.", nameof(captureId));
        }

        var token = await GetAccessTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{BaseUrl}/v2/payments/captures/{Uri.EscapeDataString(captureId)}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"PayPal capture retrieval failed: {content}");
        }

        using var document = JsonDocument.Parse(content);
        return document.RootElement.Clone();
    }


}