using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;

namespace ExpertEase.Application.DataTransferObjects.SpecialistDTOs;

public class SpecialistDTO
{
    [Required]
    public string PhoneNumber { get; set; } = null!;
    [Required]
    public string Address { get; set; } = null!;
    [Required]
    public int YearsExperience { get; set; }
    [Required]
    public string Description { get; set; } = null!;
    [Required]
    public List<CategoryDTO> Categories { get; set; } = null!;
}