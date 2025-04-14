using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Specifications;

public class AccountProjectionSpec : Specification<Account, AccountDTO>
{
    public AccountProjectionSpec(Guid id)
    {
        Query.Select(e => new AccountDTO
        {
            Id = e.Id,
            Currency = e.Currency,
            Balance = e.Balance,
        }).Where(e => e.Id == id);
    }
}

public class AccountUserProjectionSpec : Specification<Account, AccountDTO>
{
    public AccountUserProjectionSpec(Guid id)
    {
        Query.Select(e => new AccountDTO
        {
            Id = e.Id,
            Currency = e.Currency,
            Balance = e.Balance,
        }).Where(e => e.UserId == id);
    }
}