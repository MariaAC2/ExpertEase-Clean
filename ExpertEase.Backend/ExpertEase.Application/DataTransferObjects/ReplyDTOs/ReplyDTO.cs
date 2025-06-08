using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.ReplyDTOs;

public class ReplyDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public StatusEnum Status { get; set; }
}