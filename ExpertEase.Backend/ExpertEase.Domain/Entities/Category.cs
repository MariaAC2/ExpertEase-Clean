namespace ExpertEase.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public ICollection<Specialist> Specialists { get; set; } = new List<Specialist>();
    
    public override string ToString()
    {
        return 
            $"Id: {Id}, Category: {Name}" + 
               (!string.IsNullOrWhiteSpace(Description) ? $" — {Description}" : string.Empty);
    }
}