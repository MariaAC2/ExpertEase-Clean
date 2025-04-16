namespace ExpertEase.Application.DataTransferObjects.CategoryDTOs;

public class CategoryAddDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CategorySpecialistDTO
{
    public string Name { get; set; } = null!;
}