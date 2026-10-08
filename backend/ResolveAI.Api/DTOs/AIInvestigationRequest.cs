namespace ResolveAI.Api.DTOs;

public class AIInvestigationRequest
{
    public int PaymentId { get; set; }

    public string Question { get; set; } = string.Empty;
}