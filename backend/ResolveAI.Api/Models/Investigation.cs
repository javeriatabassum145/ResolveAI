using System.Text.Json.Serialization;

namespace ResolveAI.Api.Models;

public class Investigation
{
    public int Id { get; set; }

    public int PaymentId { get; set; }

    public string Question { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public string? Summary { get; set; }

    public string? RootCause { get; set; }

    public string? Recommendation { get; set; }

    public decimal? Confidence { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public Payment? Payment { get; set; }
}