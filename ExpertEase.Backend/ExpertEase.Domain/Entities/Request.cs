using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

public class Request: BaseEntity
{
    public Guid SenderUserId { get; set; }
    public User SenderUser { get; set; } = null!;
    public Guid ReceiverUserId { get; set; }
    public User ReceiverUser { get; set; } = null!;
    public DateTime RequestedStartDate { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime? RejectedAt { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Pending;
    
    public ICollection<Reply> Replies { get; set; } = new List<Reply>();
}