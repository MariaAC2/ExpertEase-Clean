using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.TransactionDTOs;

public class TransactionDTO
{
    public Guid Id { get; set; }
    public Guid InitiatorUserId { get; set; }
    public UserTransactionDTO InitiatorUser { get; set; } = null!;
    public Guid? SenderUserId { get; set; }
    public UserTransactionDTO? SenderUser { get; set; }
    public Guid? ReceiverUserId { get; set; }
    public UserTransactionDTO? ReceiverUser { get; set; }
    public TransactionEnum TransactionType { get; set; }
    public string? ExternalSource { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public StatusEnum Status { get; set; }
    public RejectionReason? RejectionCode { get; set; }
    public string? RejectionDetails { get; set; }
}