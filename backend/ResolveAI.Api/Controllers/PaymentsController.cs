using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResolveAI.Api.Data;
using ResolveAI.Api.Models;

namespace ResolveAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PaymentsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/payments
    [HttpGet]
    public async Task<IActionResult> GetPayments()
    {
        var payments = await _context.Payments
            .Include(p => p.Refunds)
            .Include(p => p.Events)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(payments);
    }

    // GET /api/payments/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPayment(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Refunds)
            .Include(p => p.Events)
            .Include(p => p.Investigations)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
        {
            return NotFound(new
            {
                message = "Payment not found."
            });
        }

        return Ok(payment);
    }

    // POST /api/payments
    [HttpPost]
    public async Task<IActionResult> CreatePayment(
        [FromBody] Payment payment)
    {
        if (payment.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Payment amount must be greater than zero."
            });
        }

        payment.Id = 0;
        payment.CreatedAt = DateTime.UtcNow;

        _context.Payments.Add(payment);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetPayment),
            new { id = payment.Id },
            payment);
    }
}