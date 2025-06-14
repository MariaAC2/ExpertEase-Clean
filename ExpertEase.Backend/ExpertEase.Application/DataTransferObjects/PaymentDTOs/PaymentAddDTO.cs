namespace ExpertEase.Application.DataTransferObjects.PaymentDTOs;

public class PaymentAddDTO
{
    public Guid ServiceTaskId { get; set; }
    public decimal Amount { get; set; }
    public string StripeAccountId { get; set; } = null!;
}

public class PaymentIntentCreateDTO
{
    public Guid ServiceTaskId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ron";
    public string Description { get; set; } = null!;
    public Dictionary<string, string>? Metadata { get; set; }
}

public class PaymentIntentResponseDTO
{
    public string ClientSecret { get; set; } = null!;
    public string PaymentIntentId { get; set; } = null!;
}

// PaymentConfirmationDTO.cs
public class PaymentConfirmationDTO
{
    public string PaymentIntentId { get; set; } = null!;
    public Guid ServiceTaskId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
}

// PaymentHistoryDTO.cs
public class PaymentHistoryDTO
{
    public Guid Id { get; set; }
    public Guid ServiceTaskId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? PaidAt { get; set; }
    public string ServiceDescription { get; set; } = null!;
    public string ServiceAddress { get; set; } = null!;
    public string SpecialistName { get; set; } = null!;
    public string ClientName { get; set; } = null!;
}

// PaymentDetailsDTO.cs
public class PaymentDetailsDTO
{
    public Guid Id { get; set; }
    public Guid ServiceTaskId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string ServiceDescription { get; set; } = null!;
    public string ServiceAddress { get; set; } = null!;
    public DateTime ServiceStartDate { get; set; }
    public DateTime ServiceEndDate { get; set; }
    public string SpecialistName { get; set; } = null!;
    public string ClientName { get; set; } = null!;
}

// PaymentRefundDTO.cs
public class PaymentRefundDTO
{
    public Guid PaymentId { get; set; }
    public decimal? Amount { get; set; } // If null, full refund
    public string? Reason { get; set; }
}