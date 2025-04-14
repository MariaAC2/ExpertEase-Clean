namespace ExpertEase.Application.DataTransferObjects.AccountDTOs;

public class AccountAddDTO
{
    public Guid UserId { get; set; }
    public string Currency { get; set; } = null!;
    public decimal InitialBalance { get; set; } = 0;
}