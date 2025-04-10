namespace ExpertEase.Application.DataTransferObjects;

public class AccountAddDTO
{
    public Guid UserId { get; set; }
    public decimal InitialBalance { get; set; } = 0;
}