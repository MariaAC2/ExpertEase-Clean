using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;

public class ServiceTaskUpdateDTO
{
    [Required]
    public Guid Id { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Address { get; set; }
    public decimal? Price { get; set; }
}