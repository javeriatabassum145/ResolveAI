using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResolveAI.Api.Data;

namespace ResolveAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var payments =
            await _context.Payments.CountAsync();

        var investigations =
            await _context.Investigations.CountAsync();

        var anomalies =
            await _context.Investigations
                .CountAsync(i =>
                    i.Status != "NoAnomaly");

        var highRisk =
            await _context.Investigations
                .CountAsync(i =>
                    i.RiskLevel == "HIGH" ||
                    i.RiskLevel == "CRITICAL");

        return Ok(new
        {
            payments,
            investigations,
            anomalies,
            highRisk
        });
    }
}