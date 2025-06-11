namespace ExpertEase.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public List<Guid> ParticipantIds { get; set; } = [];
    public Guid? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public Dictionary<string, int>? UnreadCounts { get; set; }
    public Guid RequestId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}