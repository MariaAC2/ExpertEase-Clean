using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.TransactionDTOs;

public class TransactionDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public Guid InitiatorUserId { get; set; }
    [Required]
    public UserTransactionDTO InitiatorUser { get; set; } = null!;
    public Guid? SenderUserId { get; set; }
    public UserTransactionDTO? SenderUser { get; set; }
    public Guid? ReceiverUserId { get; set; }
    public UserTransactionDTO? ReceiverUser { get; set; }
    [Required]
    public TransactionEnum TransactionType { get; set; }
    public string? ExternalSource { get; set; }
    [Required]
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    [Required]
    public StatusEnum Status { get; set; }
    public RejectionReason? RejectionCode { get; set; }
    public string? RejectionDetails { get; set; }
}