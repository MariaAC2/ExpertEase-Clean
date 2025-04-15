namespace ExpertEase.Application.DataTransferObjects.RequestDTOs;

public class RequestAddDTO
{
    public Guid ReceiverUserId { get; set; }
    public DateTime RequestedStartDate { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
}