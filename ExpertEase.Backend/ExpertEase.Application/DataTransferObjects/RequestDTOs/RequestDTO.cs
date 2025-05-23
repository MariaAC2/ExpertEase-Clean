using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.RequestDTOs;

public class RequestDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public Guid SenderUserId { get; set; }
    [Required]
    public Guid ReceiverUserId { get; set; }
    [Required]
    public DateTime RequestedStartDate { get; set; }
    [Required]
    public string Description { get; set; } = null!;
    public ContactInfoDTO? SenderContactInfo { get; set; }
    [Required]
    public StatusEnum Status { get; set; } = StatusEnum.Pending;
    
    public List<ReplyDTO> Replies { get; set; } = new List<ReplyDTO>();
}