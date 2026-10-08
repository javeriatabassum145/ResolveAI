namespace ResolveAI.Api.Models;

public class Payment
{
    public int Id { get; set; }

    public string PayPalOrderId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "EUR";

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public ICollection<PaymentEvent> Events { get; set; } = new List<PaymentEvent>();

    public ICollection<Investigation> Investigations { get; set; } = new List<Investigation>();
}