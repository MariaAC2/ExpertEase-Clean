using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.ReplyDTOs;

public class ReplyAddDTO
{
    public DateTime? StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
}