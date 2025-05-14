using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.CategoryDTOs;

public class CategoryUpdateDTO
{
    [Required]
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}