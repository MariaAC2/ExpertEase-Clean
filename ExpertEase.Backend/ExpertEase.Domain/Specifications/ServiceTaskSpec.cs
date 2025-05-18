using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class ServiceTaskSpec: Specification<ServiceTask>
{
    public ServiceTaskSpec(Guid userId)
    {
        Query.Where(e => e.UserId == userId);
    }
}