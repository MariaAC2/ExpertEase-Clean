using ExpertEase.Application.DataTransferObjects.UserDTOs;

namespace ExpertEase.Application.DataTransferObjects.ReviewDTOs;

public class ReviewDTO
{
    public Guid ReceiverUserId { get; set; }
    public string SenderUserFullName { get; set; } = null!;
    public int Rating { get; set; }
    public string Content { get; set; } = null!;
}

public class ReviewAdminDTO
{
    public Guid Id { get; set; }
    public UserDTO SenderUser { get; set; } = null!;
    public UserDTO ReceiverUser { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int Rating { get; set; }
}