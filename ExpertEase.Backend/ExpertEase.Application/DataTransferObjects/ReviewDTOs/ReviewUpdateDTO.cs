using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.ReviewDTOs;

public class ReviewUpdateDTO
{
    [Required]
    public Guid Id { get; set; }
    public string? Content { get; set; } = null!;
    public int? Rating { get; set; }
}