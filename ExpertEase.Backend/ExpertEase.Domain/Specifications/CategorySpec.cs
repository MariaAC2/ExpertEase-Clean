using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class CategorySpec : Specification<Category>
{
    public CategorySpec(Guid id) => Query.Where(e => e.Id == id);

    public CategorySpec(string name)
    {
        Query.Where(e => e.Name == name);
    }
}