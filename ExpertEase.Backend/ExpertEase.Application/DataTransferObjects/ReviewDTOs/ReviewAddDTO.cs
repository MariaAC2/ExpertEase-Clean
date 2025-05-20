using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.ReviewDTOs;

public class ReviewAddDTO
{
    [Required]
    public Guid ReceiverUserId { get; set; }
    [Required]
    public string Content { get; set; } = null!;
    [Required]
    public int Rating { get; set; }
}