namespace ExpertEase.Domain.Entities;

public class ContactInfo: BaseEntity
{
    public Guid userId { get; set; }
    public User User { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
}