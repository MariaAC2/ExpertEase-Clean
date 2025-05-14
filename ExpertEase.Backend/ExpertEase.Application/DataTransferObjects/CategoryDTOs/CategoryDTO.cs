using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.CategoryDTOs;

public class CategoryDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CategoryAdminDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    [Required]
    public int SpecialistsCount { get; set; }
    [Required]
    public List<Guid> SpecialistIds { get; set; } = null!;
}