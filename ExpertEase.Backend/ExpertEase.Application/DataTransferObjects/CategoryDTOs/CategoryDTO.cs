namespace ExpertEase.Application.DataTransferObjects.CategoryDTOs;

public class CategoryDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CategoryAdminDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SpecialistsCount { get; set; }
    public List<Guid> SpecialistIds { get; set; } = null!;
}