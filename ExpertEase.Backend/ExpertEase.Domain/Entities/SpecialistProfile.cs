namespace ExpertEase.Domain.Entities;

public class SpecialistProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public int YearsExperience { get; set; }
    public string Description { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}