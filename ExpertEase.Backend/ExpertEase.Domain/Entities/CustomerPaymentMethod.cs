namespace ExpertEase.Domain.Entities;

public class CustomerPaymentMethod: BaseEntity
{
    public Guid CustomerId { get; set; }
    public User Customer { get; set; }
    public string StripeCustomerId { get; set; } = string.Empty;
    public string StripePaymentMethodId { get; set; } = string.Empty;
    public string CardLast4 { get; set; } = string.Empty;
    public string CardBrand { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}