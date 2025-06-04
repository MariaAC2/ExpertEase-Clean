namespace ExpertEase.Application.DataTransferObjects.MessageDTOs;

public class MessageDTO
{
    public string Id { get; set; } = null!;
    public string SenderId { get; set; } = null!;
    public string ReceiverId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}