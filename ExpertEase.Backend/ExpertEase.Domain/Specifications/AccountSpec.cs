using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class AccountSpec: Specification<Account>
{
    public AccountSpec(Guid id) => Query.Where(e => e.UserId == id);
}