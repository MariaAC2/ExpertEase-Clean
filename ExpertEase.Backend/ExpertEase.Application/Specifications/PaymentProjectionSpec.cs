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
            ServiceTaskId = x.ServiceTaskId,
            Amount = x.Amount,
            Currency = x.Currency,
            Status = x.Status.ToString(),
            PaidAt = x.PaidAt,
            CreatedAt = x.CreatedAt,
            StripePaymentIntentId = x.StripePaymentIntentId,
            ServiceDescription = x.ServiceTask.Description
        });
    }
    
    public PaymentProjectionSpec(Guid serviceTaskId, string? search)
    {
        Query.Where(x => x.ServiceTaskId == serviceTaskId);
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            var searchExpr = $"%{search.Replace(" ", "%")}%";
            
            Query.Where(p =>
                EF.Functions.ILike(p.ServiceTask.Description, searchExpr) ||
                EF.Functions.ILike(p.ServiceTask.Address, searchExpr) ||
                EF.Functions.ILike(p.Status.ToString(), searchExpr) ||
                EF.Functions.ILike(p.Amount.ToString(), searchExpr)
            );
        }
        
        Query.Select(x => new PaymentDetailsDTO
        {
            Id = x.Id,
            ServiceTaskId = x.ServiceTaskId,
            Amount = x.Amount,
            Currency = x.Currency,
            Status = x.Status.ToString(),
            PaidAt = x.PaidAt,
            CreatedAt = x.CreatedAt,
            StripePaymentIntentId = x.StripePaymentIntentId,
            ServiceDescription = x.ServiceTask.Description
        });
    }
}

public class PaymentHistoryProjectionSpec : Specification<Payment, PaymentHistoryDTO>
{
    public PaymentHistoryProjectionSpec(Guid userId, string? search)
    {
        Query.Where(x => x.ServiceTask.UserId == userId || x.ServiceTask.SpecialistId == userId);
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            var searchExpr = $"%{search.Replace(" ", "%")}%";
            
            Query.Where(p =>
                EF.Functions.ILike(p.ServiceTask.Description, searchExpr) ||
                EF.Functions.ILike(p.ServiceTask.Address, searchExpr) ||
                EF.Functions.ILike(p.Status.ToString(), searchExpr) ||
                EF.Functions.ILike(p.Amount.ToString(), searchExpr)
            );
        }
        
        Query.Select(x => new PaymentHistoryDTO
        {
            Id = x.Id,
            ServiceTaskId = x.ServiceTaskId,
            Amount = x.Amount,
            Currency = x.Currency,
            Status = x.Status.ToString(),
            PaidAt = x.PaidAt,
            ServiceDescription = x.ServiceTask.Description,
            ServiceAddress = x.ServiceTask.Address,
            SpecialistName = x.ServiceTask.Specialist.FullName,
            ClientName = x.ServiceTask.User.FullName
        });
    }
}