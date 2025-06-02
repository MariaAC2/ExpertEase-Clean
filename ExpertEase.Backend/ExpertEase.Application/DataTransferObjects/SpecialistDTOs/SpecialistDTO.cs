using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;

namespace ExpertEase.Application.DataTransferObjects.UserDTOs;

public class SpecialistDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string FullName { get; set; } = null!;
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
    [Required]
    public int Rating { get; set; } = 0;
    public List<CategoryDTO> Categories { get; set; } = null!;
}

public class SpecialistDetailsDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string FullName { get; set; } = null!;
    [Required]
    public int YearsExperience { get; set; }
    [Required]
    public string Description { get; set; } = null!;
    
    public List<CategoryDTO> Categories { get; set; } = null!;
}