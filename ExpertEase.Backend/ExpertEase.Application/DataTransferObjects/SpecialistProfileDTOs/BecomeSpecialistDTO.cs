using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.SpecialistDTOs;

public class BecomeSpecialistDTO
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string PhoneNumber { get; set; } = null!;
    [Required]
    public string Address { get; set; } = null!;
    [Required]
    public int YearsExperience { get; set; }
    [Required]
    public string Description { get; set; } = null!;
    public List<Guid>? Categories { get; set; }
}
