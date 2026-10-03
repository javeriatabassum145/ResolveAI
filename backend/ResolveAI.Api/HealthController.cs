using Microsoft.AspNetCore.Mvc;

namespace ResolveAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ResolveAI API is running",
            message = "Backend is connected successfully!"
        });
    }
}