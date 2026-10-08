using Microsoft.EntityFrameworkCore;
using ResolveAI.Api.Data;

namespace ResolveAI.Api.Services;

public class AIInvestigationService
{
    private readonly AppDbContext _context;

    public AIInvestigationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> BuildInvestigationContextAsync(
        int paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.Refunds)
            .Include(p => p.Events)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            throw new KeyNotFoundException(
                $"Payment {paymentId} was not found.");
        }

        var totalRefunded =
            payment.Refunds.Sum(r => r.Amount);

        var discrepancy =
            payment.Amount - totalRefunded;

        var context = $"""
        PAYMENT FORENSICS CASE

        Payment ID:
        {payment.Id}

        PayPal Order ID:
        {payment.PayPalOrderId}

        Original Amount:
        {payment.Amount:F2} {payment.Currency}

        Payment Status:
        {payment.Status}

        Total Refund:
        {totalRefunded:F2} {payment.Currency}

        Refund Count:
        {payment.Refunds.Count}

        Unexplained Difference:
        {discrepancy:F2} {payment.Currency}

        PAYMENT EVENTS:
        {string.Join(
            "\n",
            payment.Events.Select(e =>
                $"- {e.OccurredAt:u}: {e.EventType} - {e.Description}")
        )}

        REFUNDS:
        {string.Join(
            "\n",
            payment.Refunds.Select(r =>
                $"- {r.CreatedAt:u}: {r.Amount:F2} {r.Currency}, Status={r.Status}")
        )}
        """;

        return context;
    }
}