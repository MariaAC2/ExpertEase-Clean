namespace ExpertEase.Application.DataTransferObjects.AccountDTOs;

public class AccountDTO
{
    public Guid Id { get; set; }
    public string Currency { get; set; }
    public decimal Balance { get; set; }
}
