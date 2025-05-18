namespace ExpertEase.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public ICollection<SpecialistProfile> Specialists { get; set; } = new List<SpecialistProfile>();
    
    public override string ToString()
    {
        return 
            $"Id: {Id}, Category: {Name}" + 
               (!string.IsNullOrWhiteSpace(Description) ? $" — {Description}" : string.Empty);
    }
}