namespace ExpertEase.Domain.Entities;

public class Specialist: User
{
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public int YearsExperience { get; set; }
    public string Description { get; set; } = null!;
}