namespace ExpertEase.Domain.Entities;

public class Review: BaseEntity
{
    public Guid SenderUserId { get; set; }
    public User SenderUser { get; set; } = null!;
    public Guid ReceiverUserId { get; set; }
    public User ReceiverUser { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int Rating { get; set; }
}