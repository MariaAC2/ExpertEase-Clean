using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;

namespace ExpertEase.Application.DataTransferObjects.UserDTOs;

public class UserConversationDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string FullName { get; set; } = null!;
    [Required]
    public List<RequestDTO> Requests { get; set; } = new List<RequestDTO>();
    [Required]
    public List<MessageDTO> Messages { get; set; } = new List<MessageDTO>();
}

public class ConversationDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string FullName { get; set; } = null!;
}