using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;

namespace ExpertEase.Application.DataTransferObjects.RequestDTOs;
public class RequestAddDTO
{
    [Required]
    public Guid ReceiverUserId { get; set; }
    [Required]
    public DateTime RequestedStartDate { get; set; }
    [Required]
    public string PhoneNumber { get; set; } = null!;
    [Required]
    public string Address { get; set; } = null!;
    [Required]
    public string Description { get; set; } = null!;
}