using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

public class ServiceTask : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SpecialistId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public JobStatusEnum Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public decimal Price { get; set; }
    public Guid ReplyId { get; set; }
    public Reply Reply { get; set; } = null!;
    public Review? Review { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
}