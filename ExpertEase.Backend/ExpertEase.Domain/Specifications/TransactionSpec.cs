using Ardalis.Specification;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Domain.Specifications;

public class TransactionSpec : Specification<Transaction>
{
    public TransactionSpec(Guid Id)
    {
        Query.Where(t => t.Id == Id);
        Query.Include(t => t.SenderUser);
        Query.Include(t => t.ReceiverUser);
        Query.Include(t => t.InitiatorUser);
    }
}

// public class TransactionAccountIdSpec: Specification<Transaction>
// {
//     public TransactionAccountIdSpec(Guid id)
//     {
//         Query.Where(t => t.SenderAccountId == id || t.ReceiverAccountId == id);
//         Query.Include(t => t.SenderUser);
//         Query.Include(t => t.ReceiverUser);
//     }
// }