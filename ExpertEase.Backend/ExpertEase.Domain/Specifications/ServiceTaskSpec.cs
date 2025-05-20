using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class ServiceTaskSpec: Specification<ServiceTask>
{
    public ServiceTaskSpec(Guid id)
    {
        Query.Where(e => e.Id == id);
    }
}