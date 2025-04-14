using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.TransactionDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class TransactionProjectionSpec : Specification<Transaction, TransactionDTO>
{
    public TransactionProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Select(e => new TransactionDTO
        {
            Id = e.Id,
            TransactionType = e.TransactionType,
            InitiatorUserId = e.InitiatorUserId,
            InitiatorUser = new UserTransactionDTO
            {
                Id = e.InitiatorUser.Id,
                FirstName = e.InitiatorUser.FirstName,
                LastName = e.InitiatorUser.LastName,
                Email = e.InitiatorUser.Email,
            },
            SenderUserId = e.SenderUserId,
            SenderUser = e.SenderUser != null ? new UserTransactionDTO
            {
                Id = e.SenderUser.Id,
                FirstName = e.SenderUser.FirstName,
                LastName = e.SenderUser.LastName,
                Email = e.SenderUser.Email,
            } : null,
            ReceiverUserId = e.ReceiverUserId,
            ReceiverUser = e.ReceiverUser != null ? new UserTransactionDTO
            {
                Id = e.ReceiverUser.Id,
                FirstName = e.ReceiverUser.FirstName,
                LastName = e.ReceiverUser.LastName,
                Email = e.ReceiverUser.Email,
            } : null,
            ExternalSource = e.ExternalSource,
            Amount = e.Amount,
            Description = e.Description,
            Status = e.Status,
            RejectionCode = e.RejectionCode,
            RejectionDetails = e.RejectionDetails,
        });

        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }
    
    public TransactionProjectionSpec(Guid id) : this() => Query.Where(e => e.Id == id);
    
    public TransactionProjectionSpec(string? search) : this(true) // This constructor will call the first declared constructor with 'true' as the parameter. 
    {
        search = !string.IsNullOrWhiteSpace(search) ? search.Trim() : null;
    
        if (search == null)
        {
            return;
        }
    
        var searchExpr = $"%{search.Replace(" ", "%")}%";
    
        Query.Where(e => EF.Functions.ILike(e.InitiatorUser.LastName, searchExpr)); // This is an example on how database specific expressions can be used via C# expressions.
        // Note that this will be translated to the database something like "where user.Name ilike '%str%'".
    }
}

public class TransactionUserProjectionSpec : TransactionProjectionSpec
{
    public TransactionUserProjectionSpec(Guid id) : base() => Query.Where(e => e.InitiatorUserId == id);
    
    public TransactionUserProjectionSpec(string? search) : base(search)
    {
        // Query.Where(e => e.InitiatorUserId == e.SenderUserId || e.InitiatorUserId == e.ReceiverUserId);
    }

}