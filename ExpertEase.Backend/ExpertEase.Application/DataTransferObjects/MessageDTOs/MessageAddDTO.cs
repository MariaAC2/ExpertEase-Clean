namespace ExpertEase.Application.DataTransferObjects.MessageDTOs;

public class MessageAddDTO
{
    public string Content { get; set; } = string.Empty;
    public Guid ReceiverId { get; set; }
}