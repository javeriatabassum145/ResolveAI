using System.Net.Http.Headers;
using System.Text;

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

    public async Task<string> GetAccessTokenAsync()
    {
        var clientId = _configuration["PayPal:ClientId"];
        var secret = _configuration["PayPal:Secret"];

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{clientId}:{secret}")
        );

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>(
                "grant_type",
                "client_credentials")
        });

        var response = await _httpClient.PostAsync(
            "https://api-m.sandbox.paypal.com/v1/oauth2/token",
            content);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}