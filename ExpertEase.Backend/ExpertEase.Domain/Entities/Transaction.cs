using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

public class Transaction: BaseEntity
{
    public Guid SenderUserId { get; set; }
    public User SenderUser { get; set; } = null!;
    
    public Guid ReceiverSpecialistId { get; set; }
    public Specialist ReveiverSpecialist { get; set; } = null!;
    public decimal Price { get; set; }
    public StatusEnum Status { get; set; }
}