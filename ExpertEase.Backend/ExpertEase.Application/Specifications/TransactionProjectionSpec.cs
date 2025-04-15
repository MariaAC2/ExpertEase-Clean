using Ardalis.Specification;
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
        Query.Include(t => t.InitiatorUser);
        Query.Include(t => t.SenderUser);
        Query.Include(t => t.ReceiverUser);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchExpr = $"%{search.Trim().Replace(" ", "%")}%";

            Query.Where(t =>
                EF.Functions.ILike(t.Description ?? "", searchExpr) ||
                EF.Functions.ILike(t.ExternalSource ?? "", searchExpr) ||

                // Enum as string matching
                EF.Functions.ILike(t.TransactionType.ToString(), searchExpr) ||
                EF.Functions.ILike(t.Status.ToString(), searchExpr) ||
                EF.Functions.ILike(t.RejectionCode.ToString() ?? "", searchExpr) ||

                // Initiator
                EF.Functions.ILike(t.InitiatorUser.FirstName, searchExpr) ||
                EF.Functions.ILike(t.InitiatorUser.LastName, searchExpr) ||
                EF.Functions.ILike(t.InitiatorUser.Email, searchExpr) ||

                // Sender
                (t.SenderUser != null && EF.Functions.ILike(t.SenderUser.FirstName, searchExpr)) ||
                (t.SenderUser != null && EF.Functions.ILike(t.SenderUser.LastName, searchExpr)) ||
                (t.SenderUser != null && EF.Functions.ILike(t.SenderUser.Email, searchExpr)) ||

                // Receiver
                (t.ReceiverUser != null && EF.Functions.ILike(t.ReceiverUser.FirstName, searchExpr)) ||
                (t.ReceiverUser != null && EF.Functions.ILike(t.ReceiverUser.LastName, searchExpr)) ||
                (t.ReceiverUser != null && EF.Functions.ILike(t.ReceiverUser.Email, searchExpr))
            );
        }
    }
}

public class TransactionUserProjectionSpec : TransactionProjectionSpec
{
    public TransactionUserProjectionSpec(Guid id, Guid userId) : base() => Query.Where(e => e.Id == id && e.InitiatorUserId == userId);

    public TransactionUserProjectionSpec(string? search, Guid userId) : base(search)
    {
        Query.Include(t => t.InitiatorUser);
        Query.Where(t => t.InitiatorUserId == userId);
    }
}

public class TransactionSpecialistProjectionSpec : TransactionProjectionSpec
{
    public TransactionSpecialistProjectionSpec(Guid id, Guid userId) : base() => Query.Where(e => e.Id == id && e.InitiatorUserId == userId);

    public TransactionSpecialistProjectionSpec(string? search, Guid userId) : base(search)
    {
        Query.Include(t => t.ReceiverUser);
        Query.Where(t => t.ReceiverUserId == userId);
    }
}