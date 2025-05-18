using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;

namespace ExpertEase.Application.DataTransferObjects.UserDTOs;

public class SpecialistDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string PhoneNumber { get; set; } = null!;
    [Required]
    public string Address { get; set; } = null!;
    [Required]
    public int YearsExperience { get; set; }
    [Required]
    public string Description { get; set; } = null!;
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public DateTime UpdatedAt { get; set; }
    
    public List<CategoryDTO> Categories { get; set; } = null!;
}