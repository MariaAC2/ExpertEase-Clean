namespace ExpertEase.Application.DataTransferObjects.ReviewDTOs;

public class ReviewUpdateDTO
{
    public Guid Id { get; set; }
    public string? Content { get; set; } = null!;
    public int? Rating { get; set; }
}