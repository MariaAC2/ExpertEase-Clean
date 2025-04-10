namespace ExpertEase.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public decimal Balance { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}