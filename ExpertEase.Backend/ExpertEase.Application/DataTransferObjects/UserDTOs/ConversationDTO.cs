using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;

namespace ExpertEase.Application.DataTransferObjects.UserDTOs;

public class ConversationDTO
{
    public Guid ConversationId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string UserFullName { get; set; } = null!;
    [Required]
    public string? UserProfilePictureUrl { get; set; } = null!;
    [Required]
    public List<RequestDTO> Requests { get; set; } = new List<RequestDTO>();
    [Required]
    public List<MessageDTO> Messages { get; set; } = new List<MessageDTO>();
}

public class UserConversationDTO
{
    [Required]
    public string UserId { get; set; }
    [Required]
    public string UserFullName { get; set; } = null!;
    [Required]
    public string? UserProfilePictureUrl { get; set; } = null!;
}