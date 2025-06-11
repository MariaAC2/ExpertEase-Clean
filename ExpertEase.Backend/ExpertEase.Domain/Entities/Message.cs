namespace ExpertEase.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; } = false;
    public Photo? Attachment { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}