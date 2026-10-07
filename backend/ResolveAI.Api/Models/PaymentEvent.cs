namespace ResolveAI.Api.Models;

public class PaymentEvent
{
    public int Id { get; set; }

    public int PaymentId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public Payment Payment { get; set; } = null!;
}