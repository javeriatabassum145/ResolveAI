using Microsoft.EntityFrameworkCore;
using ResolveAI.Api.Data;
using ResolveAI.Api.Models;

namespace ResolveAI.Api.Services;

public class InvestigationService
{
    private readonly AppDbContext _context;

    public InvestigationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Investigation> InvestigateAsync(
        int paymentId,
        string question)
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

        var findings = new List<string>();

        string status;
        string summary;
        string rootCause;
        string recommendation;
        decimal confidence;

        // Rule 1: Refund exceeds payment
        if (totalRefunded > payment.Amount)
        {
            status = "AnomalyDetected";

            findings.Add(
                "Total refunded amount is greater than the original payment.");

            summary =
                $"The payment was {payment.Amount:F2} {payment.Currency}, " +
                $"but {totalRefunded:F2} {payment.Currency} was refunded.";

            rootCause =
                "The refund amount exceeds the original payment amount.";

            recommendation =
                "Review all refund transactions and verify whether an " +
                "incorrect or duplicate refund was processed.";

            confidence = 0.99m;
        }

        // Rule 2: Partial refund
        else if (totalRefunded > 0 &&
                 totalRefunded < payment.Amount)
        {
            status = "DiscrepancyDetected";

            findings.Add(
                "Refund amount is lower than the original payment.");

            summary =
                $"The payment was {payment.Amount:F2} {payment.Currency}, " +
                $"but only {totalRefunded:F2} {payment.Currency} was refunded.";

            rootCause =
                $"There is a {discrepancy:F2} {payment.Currency} " +
                "difference between the payment and refund.";

            recommendation =
                "Verify whether the remaining amount was intentionally " +
                "not refunded or whether another refund is missing.";

            confidence = 0.95m;
        }

        // Rule 3: Multiple refunds
        else if (payment.Refunds.Count > 1)
        {
            status = "RequiresInvestigation";

            findings.Add(
                "Multiple refund transactions were detected.");

            summary =
                $"The payment contains {payment.Refunds.Count} refund transactions.";

            rootCause =
                "Multiple refund events require verification.";

            recommendation =
                "Review the refund timeline and confirm that each refund " +
                "was authorized.";

            confidence = 0.90m;
        }

        // Rule 4: Failed payment
        else if (payment.Status.Equals(
                     "FAILED",
                     StringComparison.OrdinalIgnoreCase))
        {
            status = "RequiresInvestigation";

            findings.Add(
                "The payment has a failed status.");

            summary =
                "The payment did not complete successfully.";

            rootCause =
                "PayPal reported the payment as failed.";

            recommendation =
                "Review PayPal payment events and determine why the " +
                "payment failed.";

            confidence = 0.98m;
        }

        // Rule 5: No refund / normal payment
        else
        {
            status = "NoAnomaly";

            findings.Add(
                "No refund discrepancy was detected.");

            summary =
                $"The payment of {payment.Amount:F2} {payment.Currency} " +
                "does not currently show a refund discrepancy.";

            rootCause =
                "No deterministic payment anomaly was detected.";

            recommendation =
                "No immediate action is required.";

            confidence = 0.85m;
        }

        var investigation = new Investigation
        {
            PaymentId = payment.Id,
            Question = question,
            Status = status,
            Summary = summary,
            RootCause = rootCause,
            Recommendation = recommendation,
            Confidence = confidence
        };

        _context.Investigations.Add(investigation);

        await _context.SaveChangesAsync();

        return investigation;
    }
}