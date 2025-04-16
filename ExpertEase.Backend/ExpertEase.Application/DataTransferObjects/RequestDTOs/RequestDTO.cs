using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.RequestDTOs;

public class RequestDTO
{
    public Guid Id { get; set; }
    public DateTime RequestedStartDate { get; set; }
    public string Description { get; set; } = null!;
    public StatusEnum Status { get; set; } = StatusEnum.Pending;
    public UserContactInfoDTO? SenderUser { get; set; }
    public UserContactInfoDTO ReceiverUser { get; set; }
    public DateTime? RejectedAt { get; set; }
}