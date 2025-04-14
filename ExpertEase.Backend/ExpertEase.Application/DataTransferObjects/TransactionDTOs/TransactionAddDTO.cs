using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.TransactionDTOs;

public class TransactionAddDTO
{
    public Guid? SenderUserId { get; set; }
    public Guid? ReceiverUserId { get; set; }
    public TransactionEnum TransactionType { get; set; }
    public string? ExternalSource { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}