namespace ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;

public class ServiceTaskAddDTO
{
    public Guid UserId { get; set; }
    public Guid SpecialistId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; } = null!;
    public string Address { get; set; }
    public decimal Price { get; set; }
}