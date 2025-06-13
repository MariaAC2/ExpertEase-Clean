using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Infrastructure.Firestore.FirestoreDTOs;

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
    public List<FirestoreRequestDTO> Requests { get; set; } = new();
    [Required]
    public List<FirestoreReplyDTO> Replies { get; set; } = new();
    [Required]
    public List<FirestoreMessageDTO> Messages { get; set; } = new();
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