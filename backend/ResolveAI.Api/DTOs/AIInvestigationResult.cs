namespace ResolveAI.Api.DTOs;

public class AIInvestigationResult
{
    public string RiskLevel { get; set; } = string.Empty;

    public int RiskScore { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string RootCause { get; set; } = string.Empty;

    public List<string> Evidence { get; set; } = new();

    public string Recommendation { get; set; } = string.Empty;

    public decimal Confidence { get; set; }
}