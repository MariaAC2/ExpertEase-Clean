using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.TransactionDTOs;

public class TransactionUpdateDTO
{
    public Guid Id { get; set; }
    public Guid InitiatorUserId { get; set; }
    public StatusEnum? Status { get; set; } = StatusEnum.Pending;
    public string? Description { get; set; }
}