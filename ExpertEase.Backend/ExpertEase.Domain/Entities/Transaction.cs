using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

public class Transaction: BaseEntity
{
    public Guid InitiatorUserId { get; set; }
    public User InitiatorUser { get; set; } = null!;
    public Guid? SenderUserId { get; set; }
    public User? SenderUser { get; set; }
    public Guid? ReceiverUserId { get; set; }
    public User? ReceiverUser { get; set; }
    public decimal Amount { get; set; }
    public TransactionEnum TransactionType { get; set; } = TransactionEnum.Initial;
    public string? ExternalSource { get; set; }
    public string? Description { get; set; }
    public string Summary { get; set; } = null!;
    public StatusEnum Status { get; set; }
    public Guid? SenderAccountId { get; set; }
    public Account? SenderAccount { get; set; }
    public Guid? ReceiverAccountId { get; set; }
    public Account? ReceiverAccount { get; set; }
    public DateTime? RejectedAt { get; set; }
    public RejectionReason? RejectionCode { get; set; }
    public string? RejectionDetails { get; set; }
    
    public override string ToString()
    {
        var senderName = SenderUser != null ? $"{SenderUser.FirstName} {SenderUser.LastName}" : "N/A";
        var receiverName = ReceiverUser != null ? $"{ReceiverUser.FirstName} {ReceiverUser.LastName}" : "N/A";
        var initiatorName = InitiatorUser != null ? $"{InitiatorUser.FirstName} {InitiatorUser.LastName}" : "N/A";

        return
            $"Transaction [{Id}]:\n" +
            $"- Initiator: {initiatorName} (ID: {InitiatorUserId})\n" +
            $"- Sender: {senderName} (ID: {SenderUserId?.ToString() ?? "N/A"})\n" +
            $"- Receiver: {receiverName} (ID: {ReceiverUserId?.ToString() ?? "N/A"})\n" +
            $"- Type: {TransactionType}\n" +
            $"- Amount: {Amount:C}\n" +
            $"- Status: {Status}\n" +
            $"- Description: {Description ?? "N/A"}\n" +
            $"- External Source: {ExternalSource ?? "N/A"}\n" +
            $"- Rejected At: {(RejectedAt.HasValue ? RejectedAt.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}\n" +
            $"- Rejection Code: {RejectionCode?.ToString() ?? "N/A"}\n" +
            $"- Rejection Details: {RejectionDetails ?? "N/A"}\n" +
            $"- Summary: {Summary}";
    }
}