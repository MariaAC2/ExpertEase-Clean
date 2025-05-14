using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.RequestDTOs;

public class RequestDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime RequestedStartDate { get; set; }
    [Required]
    public string Description { get; set; } = null!;
    [Required]
    public StatusEnum Status { get; set; } = StatusEnum.Pending;
    public UserContactInfoDTO? SenderUser { get; set; }
    [Required]
    public UserContactInfoDTO ReceiverUser { get; set; }
    public DateTime? RejectedAt { get; set; }
}