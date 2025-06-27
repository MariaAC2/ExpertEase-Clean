using System.Text.Json;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid ReplyId { get; set; }
    public Reply Reply { get; set; } = null!;
    
    // ✅ UPDATED: Separate amounts for transparency
    public decimal ServiceAmount { get; set; }      // Amount specialist will receive
    public decimal ProtectionFee { get; set; }     // Platform's protection fee
    public decimal TotalAmount { get; set; }       // ServiceAmount + ProtectionFee
    public string StripeAccountId { get; set; } = null!;
    public string? StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }
    
    // ✅ NEW: Transfer and refund tracking
    public string? StripeTransferId { get; set; }
    public string? StripeRefundId { get; set; }
    
    // ✅ UPDATED: Enhanced status enum
    public PaymentStatusEnum Status { get; set; } = PaymentStatusEnum.Pending;
    
    // ✅ NEW: Additional timestamps for escrow tracking
    public DateTime? PaidAt { get; set; }           // When client paid
    public DateTime? EscrowReleasedAt { get; set; } // When money sent to specialist
    public DateTime? CancelledAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    
    public string? Currency { get; set; } = "RON";
    
    // ✅ NEW: Fee calculation details stored as JSON (domain layer only stores the string)
    public string? ProtectionFeeDetailsJson { get; set; }
    
    // ✅ NEW: Financial tracking
    public decimal TransferredAmount { get; set; } = 0;  // Amount actually sent to specialist
    public decimal RefundedAmount { get; set; } = 0;     // Amount refunded to client
    public decimal PlatformRevenue { get; set; } = 0;    // Platform's actual revenue
    public bool FeeCollected { get; set; } = false;     // Whether platform fee is secured
    
    public Guid? ServiceTaskId { get; set; }
    
    // ✅ REMOVED: GetProtectionFeeDetails and SetProtectionFeeDetails methods
    // These are now handled by helper classes in the infrastructure layer
    
    // ✅ NEW: Business logic helpers (domain-only, no DTO dependencies)
    public bool CanBeReleased => Status == PaymentStatusEnum.Completed && TransferredAmount == 0;
    public bool CanBeRefunded => Status == PaymentStatusEnum.Completed || Status == PaymentStatusEnum.Escrowed;
    public bool IsEscrowed => Status == PaymentStatusEnum.Escrowed || Status == PaymentStatusEnum.Completed;
    public decimal PendingAmount => TotalAmount - TransferredAmount - RefundedAmount;
}
