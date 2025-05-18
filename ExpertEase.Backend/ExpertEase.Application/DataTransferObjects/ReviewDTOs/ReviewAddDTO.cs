namespace ExpertEase.Application.DataTransferObjects.ReviewDTOs;

public class ReviewAddDTO
{
    public Guid ReceiverUserId { get; set; }
    public string Content { get; set; } = null!;
    public int Rating { get; set; }
}