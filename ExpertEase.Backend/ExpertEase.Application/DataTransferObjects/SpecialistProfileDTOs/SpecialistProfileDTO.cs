using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;

namespace ExpertEase.Application.DataTransferObjects.SpecialistDTOs;

public class SpecialistProfileDTO
{
    [Required]
    public int YearsExperience { get; set; }
    [Required]
    public string Description { get; set; } = null!;
    [Required]
    public List<CategoryDTO> Categories { get; set; } = null!;
}