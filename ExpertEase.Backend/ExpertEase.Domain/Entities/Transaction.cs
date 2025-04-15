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
    public StatusEnum Status { get; set; }
    public Guid? SenderAccountId { get; set; }
    public Account? SenderAccount { get; set; }
    public Guid? ReceiverAccountId { get; set; }
    public Account? ReceiverAccount { get; set; }
    public RejectionReason? RejectionCode { get; set; }
    public string? RejectionDetails { get; set; }
}