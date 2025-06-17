using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid ReplyId { get; set; }
    public Reply Reply { get; set; } = null!;
    public decimal Amount { get; set; }
    public string StripeAccountId { get; set; } = null!;
    public string? StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }
    public PaymentStatusEnum Status { get; set; } = PaymentStatusEnum.Pending;
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? Currency { get; set; } = "RON";
    
    public Guid? ServiceTaskId { get; set; }
}
