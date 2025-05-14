using System.ComponentModel.DataAnnotations;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.ReplyDTOs;

public class ReplyUpdateDTO
{
    [Required]
    public Guid Id { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Price { get; set; }
}