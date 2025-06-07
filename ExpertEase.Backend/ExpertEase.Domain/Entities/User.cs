using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

/// <summary>
/// This is an example for a user entity, it will be mapped to a single table and each property will have it's own column except for entity object references also known as navigation properties.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public UserRoleEnum Role { get; set; }
    public ContactInfo? ContactInfo { get; set; }
    public SpecialistProfile? SpecialistProfile { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public Account Account { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public int Rating = 0;
    
    public override string ToString()
    {
        return $"User: {FullName} ({Email})\n" +
               $"- Role: {Role}\n" +
               $"- Contact Info: {(ContactInfo != null ? $"{ContactInfo.PhoneNumber}, {ContactInfo.Address}" : "N/A")}\n" +
               $"- Is Specialist: {(SpecialistProfile != null ? "Yes" : "No")}\n" +
               $"- Account: {(Account != null ? $"Balance: {Account.Balance} {Account.Currency}" : "N/A")}\n" +
               $"- Rating: {Rating}\n" +
               $"- Requests: {Requests.Count}, Reviews: {Reviews.Count}, Transactions: {Transactions.Count}";
    }
}
