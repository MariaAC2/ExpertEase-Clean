using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.SpecialistDTOs;

public class SpecialistProfileUpdateDTO
{
    [Required]
    public Guid UserId { get; set; }
    public string? PhoneNumber { get; set; } = null!;
    public string? Address { get; set; } = null!;
    public int? YearsExperience { get; set; }
    public string? Description { get; set; } = null!;
}