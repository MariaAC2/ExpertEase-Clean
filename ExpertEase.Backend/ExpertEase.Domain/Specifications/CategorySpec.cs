using Ardalis.Specification;
using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Domain.Specifications;

public class CategorySpec : Specification<Category>
{
    public CategorySpec(Guid id)
    {
        Query.Where(e => e.Id == id);
    }

    public CategorySpec(string name)
    {
        Query.Where(e => EF.Functions.ILike(e.Name, name));
        Query.Include(e => e.Specialists);
    }
}