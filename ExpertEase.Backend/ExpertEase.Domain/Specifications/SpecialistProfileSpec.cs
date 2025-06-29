using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class SpecialistProfileSpec: Specification<SpecialistProfile>
{
    public SpecialistProfileSpec(Guid id)
    {
        Query.Where(e => e.UserId == id);
        Query.Include(e=> e.Categories);
        Query.Include(e => e.User);
    }
}