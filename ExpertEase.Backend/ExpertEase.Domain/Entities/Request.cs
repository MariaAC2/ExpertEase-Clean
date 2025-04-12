using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

public class Request: BaseEntity
{
    public Guid SenderUserId { get; set; }
    public User SenderUser { get; set; } = null!;
    
    public Guid ReceiverSpecialistId { get; set; }
    public Specialist ReveiverSpecialist { get; set; } = null!;
    public DateTime RequestDate { get; set; }
    public string? Description { get; set; }
    public StatusEnum Status { get; set; } = StatusEnum.Pending;
}