namespace ExpertEase.Application.DataTransferObjects;

public class AccountDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
}
