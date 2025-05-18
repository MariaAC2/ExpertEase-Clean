using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.UserDTOs;

public class SpecialistUpdateDTO
{
    [Required]
    public Guid Id { get; set; }
    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string? Address { get; set; } = null!;
    public int? YearsExperience { get; set; }
    public string? Description { get; set; } = null!;
}