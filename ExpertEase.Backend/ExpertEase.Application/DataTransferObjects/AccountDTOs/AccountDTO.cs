namespace ExpertEase.Application.DataTransferObjects.AccountDTOs;

public class AccountDTO
{
    public Guid Id { get; set; }
    public string Currency { get; set; } = null!;
    public decimal Balance { get; set; }
}
