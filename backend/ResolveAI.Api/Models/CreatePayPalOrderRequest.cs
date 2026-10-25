
namespace ResolveAI.Api.Models;

public class CreatePayPalOrderRequest
{
    public decimal Amount { get; set; } = 10.00m;
    public string Currency { get; set; } = "USD";
}
