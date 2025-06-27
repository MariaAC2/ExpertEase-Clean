namespace ExpertEase.Application.DataTransferObjects.StripeAccountDTOs;

// ✅ NEW: DTOs for enhanced payment intent creation

/// <summary>
/// DTO for creating payment intent with escrow support
/// </summary>
public class CreatePaymentIntentDTO
{
    /// <summary>
    /// Total amount charged to client (ServiceAmount + ProtectionFee)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Amount that will be transferred to specialist when service completes
    /// </summary>
    public decimal ServiceAmount { get; set; }

    /// <summary>
    /// Platform protection fee (kept by platform)
    /// </summary>
    public decimal ProtectionFee { get; set; }

    /// <summary>
    /// Specialist's Stripe account ID (for later transfer)
    /// </summary>
    public string SpecialistAccountId { get; set; } = string.Empty;

    /// <summary>
    /// Payment description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Currency (default: ron)
    /// </summary>
    public string Currency { get; set; } = "ron";

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Response DTO for payment intent creation
/// </summary>
public class CreatePaymentIntentResponseDTO
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal ServiceAmount { get; set; }
    public decimal ProtectionFee { get; set; }
    public string SpecialistAccountId { get; set; } = string.Empty;
    public string Currency { get; set; } = "RON";
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// ✅ UPDATED: Enhanced Stripe account status DTO
/// </summary>
public class StripeAccountStatusDTO
{
    public string AccountId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool ChargesEnabled { get; set; }
    public bool PayoutsEnabled { get; set; }
    public bool DetailsSubmitted { get; set; }
    public List<string> RequirementsCurrentlyDue { get; set; } = new();
    public List<string> RequirementsEventuallyDue { get; set; } = new();
    public string? DisabledReason { get; set; }
    
    // ✅ NEW: Enhanced properties
    public bool IsTestMode { get; set; }
    public bool CanReceivePayments { get; set; }
}