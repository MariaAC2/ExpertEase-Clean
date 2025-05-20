using System.ComponentModel.DataAnnotations;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.ReplyDTOs;

public class ReplyAddDTO
{
    //public Guid requestId { get; set; }
    public DateTime? StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    [Required]
    public decimal Price { get; set; }
}