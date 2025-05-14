using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.CategoryDTOs;

public class CategoryAddDTO
{
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CategorySpecialistDTO
{
    [Required]
    public string Name { get; set; } = null!;
}