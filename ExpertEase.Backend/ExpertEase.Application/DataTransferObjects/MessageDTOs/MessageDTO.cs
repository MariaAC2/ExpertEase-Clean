namespace ExpertEase.Application.DataTransferObjects.MessageDTOs;

public class MessageDTO
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}