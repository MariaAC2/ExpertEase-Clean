using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

/// <summary>
/// This is an example for a user entity, it will be mapped to a single table and each property will have it's own column except for entity object references also known as navigation properties.
/// </summary>
public class User : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public UserRoleEnum Role { get; set; }
    public Specialist? Specialist { get; set; }
    public Account Account { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
