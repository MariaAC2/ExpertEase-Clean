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
    
    public override string ToString()
    {
        var senderName = SenderUser != null ? $"{SenderUser.FirstName} {SenderUser.LastName}" : "N/A";
        var receiverName = ReceiverUser != null ? $"{ReceiverUser.FirstName} {ReceiverUser.LastName}" : "N/A";

        return
            $"Request [{Id}]:\n" +
            $"- Sender: {senderName} (ID: {SenderUserId})\n" +
            $"- Receiver: {receiverName} (ID: {ReceiverUserId})\n" +
            $"- Status: {Status}\n" +
            $"- Requested Start Date: {RequestedStartDate:yyyy-MM-dd HH:mm}\n" +
            $"- Phone: {PhoneNumber}\n" +
            $"- Address: {Address}\n" +
            $"- Description: {Description}\n" +
            $"- Rejected At: {(RejectedAt.HasValue ? RejectedAt.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}\n" +
            $"- Replies Count: {Replies?.Count ?? 0}";
    }
}