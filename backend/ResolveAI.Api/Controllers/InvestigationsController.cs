using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResolveAI.Api.Data;
using ResolveAI.Api.Services;

namespace ResolveAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvestigationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly InvestigationService _investigationService;

    public InvestigationsController(
        AppDbContext context,
        InvestigationService investigationService)
    {
        _context = context;
        _investigationService = investigationService;
    }

    // POST /api/investigations/1
    [HttpPost("{paymentId:int}")]
    public async Task<IActionResult> Investigate(
        int paymentId,
        [FromBody] InvestigationRequest? request)
    {
        var paymentExists =
            await _context.Payments.AnyAsync(
                p => p.Id == paymentId);

        if (!paymentExists)
        {
            return NotFound(new
            {
                message = "Payment not found."
            });
        }

        var question =
            request?.Question
            ?? "Why does this payment look incorrect?";

        try
        {
            var investigation =
                await _investigationService.InvestigateAsync(
                    paymentId,
                    question);

            return Ok(investigation);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Investigation failed.",
                error = ex.Message
            });
        }
    }

    // GET /api/investigations
    [HttpGet]
    public async Task<IActionResult> GetInvestigations()
    {
        var investigations = await _context.Investigations
            .Include(i => i.Payment)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(investigations);
    }

    // GET /api/investigations/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInvestigation(
        int id)
    {
        var investigation =
            await _context.Investigations
                .Include(i => i.Payment)
                .FirstOrDefaultAsync(i => i.Id == id);

        if (investigation == null)
        {
            return NotFound(new
            {
                message = "Investigation not found."
            });
        }

        return Ok(investigation);
    }
}

public class InvestigationRequest
{
    public string? Question { get; set; }
}