using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

public class Reply : BaseEntity
{
    public Guid RequestId { get; set; }
    public Request Request { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public StatusEnum Status { get; set; }
    public DateTime? RejectedAt { get; set; }
    public Payment? Payment { get; set; }
}