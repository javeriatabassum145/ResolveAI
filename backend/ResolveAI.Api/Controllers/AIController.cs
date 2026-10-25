using Microsoft.AspNetCore.Mvc;
using ResolveAI.Api.Services;

namespace ResolveAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly AIInvestigationService _aiService;

    public AIController(AIInvestigationService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("investigate/{paymentId:int}")]
    public async Task<IActionResult> Investigate(
        int paymentId,
        [FromBody] AIQuestionRequest request)
    {
        try
        {
            var result = await _aiService.InvestigateAsync(
                paymentId,
                request.Question);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "AI investigation failed.",
                error = ex.Message
            });
        }
    }
}

public class AIQuestionRequest
{
    public string Question { get; set; } = string.Empty;
}