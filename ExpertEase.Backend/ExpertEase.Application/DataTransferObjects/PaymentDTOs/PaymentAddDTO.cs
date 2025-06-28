namespace ExpertEase.Application.DataTransferObjects.PaymentDTOs;

// ✅ UPDATED: Enhanced payment history with fee breakdown
public class PaymentHistoryDTO
{
    public Guid Id { get; set; }
    public Guid ReplyId { get; set; }
    
    // ✅ NEW: Detailed amount breakdown
    public decimal ServiceAmount { get; set; }
    public decimal ProtectionFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? PaidAt { get; set; }
    public DateTime? EscrowReleasedAt { get; set; } // ✅ NEW
    public string ServiceDescription { get; set; } = null!;
    public string ServiceAddress { get; set; } = null!;
    public string SpecialistName { get; set; } = null!;
    public string ClientName { get; set; } = null!;
    
    // ✅ NEW: Financial tracking
    public decimal TransferredAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public bool IsEscrowed { get; set; }
}

// ✅ UPDATED: Enhanced payment details
public class PaymentDetailsDTO
{
    public Guid Id { get; set; }
    public Guid ReplyId { get; set; }
    
    // ✅ NEW: Detailed amount breakdown
    public decimal ServiceAmount { get; set; }
    public decimal ProtectionFee { get; set; }
    public decimal TotalAmount { get; set; }
    
    public string Currency { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? PaidAt { get; set; }
    public DateTime? EscrowReleasedAt { get; set; } // ✅ NEW
    public DateTime CreatedAt { get; set; }
    
    public string? StripePaymentIntentId { get; set; }
    public string? StripeTransferId { get; set; } // ✅ NEW
    public string? StripeRefundId { get; set; } // ✅ NEW
    
    public string ServiceDescription { get; set; } = null!;
    public string ServiceAddress { get; set; } = null!;
    public DateTime ServiceStartDate { get; set; }
    public DateTime ServiceEndDate { get; set; }
    public string SpecialistName { get; set; } = null!;
    public string ClientName { get; set; } = null!;
    
    // ✅ NEW: Financial details
    public decimal TransferredAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal PlatformRevenue { get; set; }
    public bool IsEscrowed { get; set; }
    public ProtectionFeeDetailsDTO? ProtectionFeeDetails { get; set; }
}

public class PaymentAddDTO
{
    public Guid ReplyId { get; set; }
    
    // ✅ UPDATED: Separate amounts
    public decimal ServiceAmount { get; set; }
    public decimal ProtectionFee { get; set; }
    public string StripeAccountId { get; set; } = null!;
}

// ✅ UPDATED: Enhanced payment intent creation
public class PaymentIntentCreateDTO
{
    public Guid ReplyId { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal ProtectionFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "ron";
    public string Description { get; set; } = null!;
    public Dictionary<string, string>? Metadata { get; set; }
    
    // ✅ NEW: Fee calculation details
    public ProtectionFeeDetailsDTO? ProtectionFeeDetails { get; set; }
}

// ✅ UPDATED: Enhanced response with fee details
public class PaymentIntentResponseDTO
{
    public string ClientSecret { get; set; } = null!;
    public string PaymentIntentId { get; set; } = null!;
    public string StripeAccountId { get; set; } = null!;
    public decimal ServiceAmount { get; set; }
    public decimal ProtectionFee { get; set; }
    public decimal TotalAmount { get; set; }
    public ProtectionFeeDetailsDTO? ProtectionFeeDetails { get; set; }
}

// ✅ UPDATED: Enhanced confirmation with fee tracking
public class PaymentConfirmationDTO
{
    public string PaymentIntentId { get; set; } = null!;
    public Guid ReplyId { get; set; }
    
    // ✅ NEW: Detailed amounts for verification
    public decimal ServiceAmount { get; set; }
    public decimal ProtectionFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = null!;
}

// ✅ UPDATED: Enhanced refund with partial refund support
public class PaymentRefundDTO
{
    public Guid PaymentId { get; set; }
    public decimal? Amount { get; set; } // If null, full refund
    public string? Reason { get; set; }
    
    // ✅ NEW: Specify what to refund
    public bool RefundServiceAmount { get; set; } = true;
    public bool RefundProtectionFee { get; set; } = true;
}

// ✅ NEW: Release payment to specialist
public class PaymentReleaseDTO
{
    public Guid PaymentId { get; set; }
    public string? Reason { get; set; } = "Service completed successfully";
    public decimal? CustomAmount { get; set; } // If null, release full service amount
}

// ✅ NEW: Payment status query
public class PaymentStatusResponseDTO
{
    public Guid PaymentId { get; set; }
    public string Status { get; set; } = null!;
    public bool IsEscrowed { get; set; }
    public bool CanBeReleased { get; set; }
    public bool CanBeRefunded { get; set; }
    public PaymentAmountBreakdown AmountBreakdown { get; set; } = new();
    public ProtectionFeeDetailsDTO? ProtectionFeeDetails { get; set; }
}

// ✅ NEW: Fee calculation details
public class ProtectionFeeDetailsDTO
{
    public decimal BaseServiceAmount { get; set; }
    public string FeeType { get; set; } = "percentage";
    public decimal FeePercentage { get; set; }
    public decimal FixedFeeAmount { get; set; }
    public decimal MinimumFee { get; set; }
    public decimal MaximumFee { get; set; }
    public decimal CalculatedFee { get; set; }
    public string FeeJustification { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Protection fee calculation result
/// </summary>
public class ProtectionFeeCalculation
{
    public decimal BaseAmount { get; set; }
    public string FeeType { get; set; } = string.Empty;
    public decimal PercentageRate { get; set; }
    public decimal FixedAmount { get; set; }
    public decimal MinimumFee { get; set; }
    public decimal MaximumFee { get; set; }
    public decimal CalculatedFee { get; set; }  // Before min/max applied
    public decimal FinalFee { get; set; }       // After min/max applied
    public string Justification { get; set; } = string.Empty;
}

/// <summary>
/// Payment amount breakdown
/// </summary>
public class PaymentAmountBreakdown
{
    public decimal ServiceAmount { get; set; }
    public decimal ProtectionFee { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TransferredAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal PlatformRevenue { get; set; }
    public ProtectionFeeCalculation? ProtectionFeeCalculation { get; set; }
}

public class ProtectionFeeConfig
{
    public string FeeType { get; set; } = "percentage"; // "percentage", "fixed", "hybrid"
    public decimal PercentageRate { get; set; } = 10.0m;
    public decimal FixedAmount { get; set; } = 25.0m;
    public decimal MinimumFee { get; set; } = 5.0m;
    public decimal MaximumFee { get; set; } = 100.0m;
    public bool IsEnabled { get; set; } = true;
}

public class PaymentReportDTO
{
    // ✅ Time Period
    public string Period { get; set; } = string.Empty;  // e.g., "2024-01-01 to 2024-01-31"
    
    // ✅ Revenue Breakdown
    public decimal TotalServiceRevenue { get; set; }     // All money for services (goes to specialists)
    public decimal TotalProtectionFees { get; set; }    // All platform fees collected
    public decimal TotalPlatformRevenue { get; set; }   // Platform's actual net revenue
    
    // ✅ Transaction Statistics  
    public int TotalTransactions { get; set; }          // Total number of payments
    public int CompletedServices { get; set; }          // Services that were completed
    public int RefundedServices { get; set; }           // Services that were refunded
    public int EscrowedPayments { get; set; }           // Payments currently held in escrow
    
    // ✅ Business Metrics
    public decimal RefundRate { get; set; }             // Percentage of payments refunded
    public decimal AverageServiceValue { get; set; }    // Average service price
    public decimal AverageProtectionFee { get; set; }   // Average platform fee
    public decimal TotalEscrowedAmount { get; set; }    // Total money currently in escrow
}