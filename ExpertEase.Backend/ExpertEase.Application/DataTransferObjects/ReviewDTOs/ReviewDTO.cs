using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.UserDTOs;

namespace ExpertEase.Application.DataTransferObjects.ReviewDTOs;

public class ReviewDTO
{
    [Required]
    public Guid ReceiverUserId { get; set; }
    [Required]
    public string SenderUserFullName { get; set; } = null!;
    [Required]
    public int Rating { get; set; }
    [Required]
    public string Content { get; set; } = null!;
}

public class ReviewAdminDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public Guid SenderUserId { get; set; }
    [Required]
    public Guid ReceiverUserId { get; set; }
    [Required]
    public Guid ServiceTaskId { get; set; }
    [Required]
    public string Content { get; set; } = null!;
    [Required]
    public int Rating { get; set; }
}