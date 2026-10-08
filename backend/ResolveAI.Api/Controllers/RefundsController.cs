using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResolveAI.Api.Data;
using ResolveAI.Api.Models;

namespace ResolveAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RefundsController : ControllerBase
{
    private readonly AppDbContext _context;

    public RefundsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/refunds/payment/1
    [HttpGet("payment/{paymentId:int}")]
    public async Task<IActionResult> GetRefundsForPayment(
        int paymentId)
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

        var refunds = await _context.Refunds
            .Where(r => r.PaymentId == paymentId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        return Ok(refunds);
    }

    // POST /api/refunds
    [HttpPost]
    public async Task<IActionResult> CreateRefund(
        [FromBody] Refund refund)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(
                p => p.Id == refund.PaymentId);

        if (payment == null)
        {
            return NotFound(new
            {
                message = "Payment not found."
            });
        }

        if (refund.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Refund amount must be greater than zero."
            });
        }

        refund.Id = 0;
        refund.CreatedAt = DateTime.UtcNow;

        _context.Refunds.Add(refund);

        // Add a payment event automatically
        var paymentEvent = new PaymentEvent
        {
            PaymentId = payment.Id,
            EventType = "REFUND_CREATED",
            Description =
                $"Refund of {refund.Amount:F2} {refund.Currency} created.",
            OccurredAt = DateTime.UtcNow
        };

        _context.PaymentEvents.Add(paymentEvent);

        await _context.SaveChangesAsync();

        return Ok(refund);
    }
}