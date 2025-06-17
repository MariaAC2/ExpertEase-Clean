using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class PaymentProjectionSpec : Specification<Payment, PaymentDetailsDTO>
{
    public PaymentProjectionSpec(Guid id)
    {
        Query.Where(x => x.Id == id);
        Query.Select(x => new PaymentDetailsDTO
        {
            Id = x.Id,
            ReplyId = x.ReplyId,
            Amount = x.Amount,
            Currency = x.Currency,
            Status = x.Status.ToString(),
            PaidAt = x.PaidAt,
            CreatedAt = x.CreatedAt,
            StripePaymentIntentId = x.StripePaymentIntentId,
            ServiceDescription = x.Reply.Request.Description
        });
    }
    
    public PaymentProjectionSpec(string paymentIntentId)
    {
        Query.Where(p => p.StripePaymentIntentId == paymentIntentId);
        Query.Select(x => new PaymentDetailsDTO
        {
            Id = x.Id,
            ReplyId = x.ReplyId,
            Amount = x.Amount,
            Currency = x.Currency,
            Status = x.Status.ToString(),
            PaidAt = x.PaidAt,
            CreatedAt = x.CreatedAt,
            StripePaymentIntentId = x.StripePaymentIntentId,
            ServiceDescription = x.Reply.Request.Description
        });
    }
    
    public PaymentProjectionSpec(Guid replyId, string? search)
    {
        Query.Where(x => x.ReplyId == replyId);
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            var searchExpr = $"%{search.Replace(" ", "%")}%";
            
            Query.Where(p =>
                EF.Functions.ILike(p.Reply.Request.Description, searchExpr) ||
                EF.Functions.ILike(p.Reply.Request.Address, searchExpr) ||
                EF.Functions.ILike(p.Status.ToString(), searchExpr) ||
                EF.Functions.ILike(p.Amount.ToString(), searchExpr)
            );
        }
        
        Query.Select(x => new PaymentDetailsDTO
        {
            Id = x.Id,
            ReplyId = x.ReplyId,
            Amount = x.Amount,
            Currency = x.Currency,
            Status = x.Status.ToString(),
            PaidAt = x.PaidAt,
            CreatedAt = x.CreatedAt,
            StripePaymentIntentId = x.StripePaymentIntentId,
            ServiceDescription = x.Reply.Request.Description
        });
    }
}

public class PaymentHistoryProjectionSpec : Specification<Payment, PaymentHistoryDTO>
{
    public PaymentHistoryProjectionSpec(Guid userId, string? search)
    {
        Query.Where(x => x.Reply.Request.SenderUserId == userId || x.Reply.Request.ReceiverUserId == userId);
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            var searchExpr = $"%{search.Replace(" ", "%")}%";
            
            Query.Where(p =>
                EF.Functions.ILike(p.Reply.Request.Description, searchExpr) ||
                EF.Functions.ILike(p.Reply.Request.Address, searchExpr) ||
                EF.Functions.ILike(p.Status.ToString(), searchExpr) ||
                EF.Functions.ILike(p.Amount.ToString(), searchExpr)
            );
        }
        
        Query.Select(x => new PaymentHistoryDTO
        {
            Id = x.Id,
            ReplyId = x.ReplyId,
            Amount = x.Amount,
            Currency = x.Currency,
            Status = x.Status.ToString(),
            PaidAt = x.PaidAt,
            ServiceDescription = x.Reply.Request.Description,
            ServiceAddress = x.Reply.Request.Address,
            SpecialistName = x.Reply.Request.ReceiverUser.FullName,
            ClientName = x.Reply.Request.SenderUser.FullName
        });
    }
}