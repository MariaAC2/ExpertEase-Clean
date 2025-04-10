namespace ExpertEase.Application.DataTransferObjects;

public class SpecialistAddDTO
{
    public Guid UserId { get; set; } // The existing user's ID

    // Specialist-specific fields
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public int YearsExperience { get; set; }
    public string Description { get; set; } = null!;
}
