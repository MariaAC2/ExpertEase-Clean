using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class AccountSpec: Specification<Account>
{
    public AccountSpec(Guid id) => Query.Where(e => e.Id == id);
}

// public class AccountUserSpec: Specification<Account>
// {
//     public AccountUserSpec(Guid userId) => Query.Where(e => e.UserId == userId);
// }