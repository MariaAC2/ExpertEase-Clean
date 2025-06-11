namespace ExpertEase.Application.DataTransferObjects.PaymentDTOs;

public class PaymentAddDTO
{
    public Guid ServiceTaskId { get; set; }
    public decimal Amount { get; set; }
    public string StripeAccountId { get; set; } = null!;
}