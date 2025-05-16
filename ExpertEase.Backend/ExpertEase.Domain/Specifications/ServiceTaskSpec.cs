using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class JobSpec: Specification<ServiceTask>
{
    public JobSpec(Guid userId)
    {
        Query.Where(e => e.UserId == userId);
        Query.Include(e => e.User);
        Query.Include(e => e.Specialist);
    }
}