using System.ComponentModel.DataAnnotations;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.TransactionDTOs;

public class TransactionUpdateDTO
{
    [Required]
    public Guid Id { get; set; }
    public StatusEnum? Status { get; set; } = StatusEnum.Pending;
    public string? Description { get; set; }
}