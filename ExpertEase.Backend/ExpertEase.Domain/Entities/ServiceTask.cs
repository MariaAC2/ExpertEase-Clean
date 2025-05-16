using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

public class Job : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid SpecialistId { get; set; }
    public Specialist Specialist { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; } = null!;
    public string Address { get; set; }
    public JobStatusEnum Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public decimal Price { get; set; }
}