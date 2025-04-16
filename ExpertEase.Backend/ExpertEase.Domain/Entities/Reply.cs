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
    
    public override string ToString()
    {
        return
            $"Reply [{Id}]:\n" +
            $"- Request ID: {RequestId}\n" +
            $"- Start: {StartDate:yyyy-MM-dd HH:mm}\n" +
            $"- End: {EndDate:yyyy-MM-dd HH:mm}\n" +
            $"- Price: {Price:C}\n" +
            $"- Status: {Status}";
    }
}