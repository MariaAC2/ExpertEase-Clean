using System.ComponentModel.DataAnnotations;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;

public class ServiceTaskDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public Guid ReplyId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public Guid SpecialistId { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    [Required]
    public string Description { get; set; } = null!;
    [Required]
    public string Address { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public JobStatusEnum Status { get; set; }
    [Required]
    public DateTime? CompletedAt { get; set; }
    [Required]
    public DateTime? CancelledAt { get; set; }
}