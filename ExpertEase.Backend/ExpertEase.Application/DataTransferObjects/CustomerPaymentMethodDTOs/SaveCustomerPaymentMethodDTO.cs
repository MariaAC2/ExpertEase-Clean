namespace ExpertEase.Application.DataTransferObjects.CustomerPaymentMethodDTOs;

public class SaveCustomerPaymentMethodDto
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public string CardLast4 { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = true;
}

public class CustomerPaymentMethodDto
{
    public Guid Id { get; set; }
    // ✅ FIXED: Use UserId consistently
    public Guid UserId { get; set; }
    public string StripeCustomerId { get; set; } = string.Empty;
    public string StripePaymentMethodId { get; set; } = string.Empty;
    public string CardLast4 { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}