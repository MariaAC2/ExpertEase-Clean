using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class ReplyProjectionSpec : Specification<Reply, ReplyDTO>
{
    public ReplyProjectionSpec(Guid requestId, bool orderByCreatedAt = false)
    {
        Query.Where(x => x.RequestId == requestId);
        Query.Select(x => new ReplyDTO
        {
            Id = x.Id,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            Price = x.Price,
            Status = x.Status
        });
        
        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }
    
    public ReplyProjectionSpec(Guid id, Guid requestId) => Query.Where(x => x.RequestId == requestId && x.Id == id);
    
    public ReplyProjectionSpec(string? search, Guid requestId) : this(requestId, true)
    {
        search = !string.IsNullOrWhiteSpace(search) ? search.Trim() : null;

        if (search == null)
            return;

        var searchExpr = $"%{search.Replace(" ", "%")}%";

        Query.Where(r =>
            EF.Functions.ILike(r.Status.ToString(), searchExpr) ||
            EF.Functions.ILike(r.Price.ToString(), searchExpr) ||
            EF.Functions.ILike(r.StartDate.ToString(), searchExpr) ||
            EF.Functions.ILike(r.EndDate.ToString(), searchExpr)
        );
    }
}

public class ReplyUserProjectionSpec : ReplyProjectionSpec
{
    public ReplyUserProjectionSpec(Guid id,  Guid requestId, Guid userId) : base(id, requestId)
    {
        Query.Include(e => e.Request);
        Query.Where(x => x.Request.SenderUserId == userId);
    }

    public ReplyUserProjectionSpec(string? search, Guid requestId, Guid userId) : base(search, requestId)
    {
        Query.Include(e => e.Request);
        Query.Where(x => x.Request.SenderUserId == userId);
    }
}

public class ReplySpecialistProjectionSpec : ReplyProjectionSpec
{
    public ReplySpecialistProjectionSpec(Guid id,  Guid requestId, Guid userId) : base(id, requestId)
    {
        Query.Include(e => e.Request);
        Query.Where(x => x.Request.ReceiverUserId == userId);
    }
    
    public ReplySpecialistProjectionSpec(string? search, Guid requestId, Guid userId) : base(search, requestId)
    {
        Query.Include(e => e.Request);
        Query.Where(x => x.Request.ReceiverUserId == userId);
    }
}