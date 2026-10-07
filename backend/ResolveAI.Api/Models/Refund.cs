namespace ResolveAI.Api.Models;

public class Refund
{
    public int Id { get; set; }

    public int PaymentId { get; set; }

    public string PayPalRefundId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "EUR";

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Payment Payment { get; set; } = null!;
}