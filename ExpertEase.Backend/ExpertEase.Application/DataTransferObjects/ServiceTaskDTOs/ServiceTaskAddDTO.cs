namespace ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;

public class ServiceTaskAddDTO
{
    public Guid UserId { get; set; }
    public Guid SpecialistId { get; set; }
    public Guid ReplyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public decimal Price { get; set; }
}