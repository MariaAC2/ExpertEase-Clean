using ExpertEase.Application.DataTransferObjects.CategoryDTOs;

namespace ExpertEase.Application.DataTransferObjects.SpecialistDTOs;

public class SpecialistDTO
{
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = null!;
    public int YearsExperience { get; set; }
    public string Description { get; set; } = null!;
    public List<CategoryDTO> Categories { get; set; } = null!;
}

public class CategoriesDTO
{
    public List<CategoryDTO> Categories { get; set; } = null!;
}