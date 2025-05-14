using System.ComponentModel.DataAnnotations;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.TransactionDTOs;

public class TransactionAddDTO
{
    public Guid? SenderUserId { get; set; }
    public Guid? ReceiverUserId { get; set; }
    [Required]
    public TransactionEnum TransactionType { get; set; }
    public string? ExternalSource { get; set; }
    [Required]
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}