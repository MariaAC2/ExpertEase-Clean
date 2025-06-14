using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Infrastructure.Extensions;

public static class PaymentExtensions
{
    /// <summary>
    /// Converts amount from RON to smallest currency unit (bani)
    /// </summary>
    public static long ToStripeAmount(this decimal amount)
    {
        return (long)(amount * 100);
    }

    /// <summary>
    /// Converts amount from smallest currency unit (bani) to RON
    /// </summary>
    public static decimal FromStripeAmount(this long amount)
    {
        return amount / 100m;
    }

    /// <summary>
    /// Checks if payment can be refunded
    /// </summary>
    public static bool CanBeRefunded(this Payment payment)
    {
        return payment.Status == PaymentStatusEnum.Completed && 
               payment.PaidAt.HasValue &&
               payment.PaidAt.Value.AddDays(30) > DateTime.UtcNow; // 30-day refund window
    }

    /// <summary>
    /// Checks if payment can be cancelled
    /// </summary>
    public static bool CanBeCancelled(this Payment payment)
    {
        return payment.Status == PaymentStatusEnum.Pending;
    }

    /// <summary>
    /// Gets user-friendly status message
    /// </summary>
    public static string GetStatusMessage(this PaymentStatusEnum status)
    {
        return status switch
        {
            PaymentStatusEnum.Pending => "În așteptare",
            PaymentStatusEnum.Processing => "Se procesează",
            PaymentStatusEnum.Completed => "Finalizată",
            PaymentStatusEnum.Failed => "Eșuată",
            PaymentStatusEnum.Cancelled => "Anulată",
            PaymentStatusEnum.Refunded => "Rambursată",
            PaymentStatusEnum.PartiallyRefunded => "Parțial rambursată",
            _ => "Necunoscută"
        };
    }

    /// <summary>
    /// Calculates protection fee based on service amount
    /// </summary>
    public static decimal CalculateProtectionFee(this decimal serviceAmount)
    {
        // Fixed fee for now, but could be percentage-based
        return 25m;
    }

    /// <summary>
    /// Calculates total amount including protection fee
    /// </summary>
    public static decimal CalculateTotalAmount(this decimal serviceAmount)
    {
        return serviceAmount + serviceAmount.CalculateProtectionFee();
    }
}