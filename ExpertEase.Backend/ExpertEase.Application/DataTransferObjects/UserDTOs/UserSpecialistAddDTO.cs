namespace ExpertEase.Application.DataTransferObjects.UserDTOs;

public class UserSpecialistAddDTO
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int YearsExperience { get; set; }
    public string Description { get; set; } = null!;
    
}