using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class TransactionIdSpec : Specification<Transaction>
{
    public TransactionIdSpec(Guid Id) => Query.Where(t => t.Id == Id);
}

public class TransactionAccountIdSpec: Specification<Transaction>
{
    public TransactionAccountIdSpec(Guid id)
    {
        Query.Where(t => t.SenderAccountId == id || t.ReceiverAccountId == id);
        Query.Include(t => t.SenderUser);
        Query.Include(t => t.ReceiverUser);
    }
}