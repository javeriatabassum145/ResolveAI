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
        int paymentId)
    {
        try
        {
            var context =
                await _aiService
                    .BuildInvestigationContextAsync(paymentId);

            return Ok(new
            {
                paymentId,
                context
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }
}