using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.ReviewDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class ReviewProjectionSpec : Specification<Review, ReviewDTO>
{
    public ReviewProjectionSpec(Guid userId, bool orderByCreatedAt = false)
    {
        Query.Include(e => e.SenderUser);
        Query.Include(e => e.ReceiverUser);
        Query.Where(e=> e.ReceiverUserId == userId);
        Query.Select(x => new ReviewDTO
        {
            ReceiverUserId = x.ReceiverUserId,
            SenderUserFullName = x.SenderUser.FullName,
            Rating = x.Rating,
            Content = x.Content
        });

        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }
    
    public ReviewProjectionSpec(Guid id, Guid userId) : this(userId) => Query.Where(e => e.Id == id);
    
    public ReviewProjectionSpec(string? search, Guid userId) : this(userId, true) 
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchExpr = $"%{search.Trim().Replace(" ", "%")}%";

            Query.Where(r =>
                EF.Functions.ILike(r.Content, searchExpr) ||
                EF.Functions.ILike(r.Rating.ToString(), searchExpr)
            );
        }
    }
}

public class ReviewAdminProjectionSpec : Specification<Review, ReviewAdminDTO>
{
    public ReviewAdminProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Include(e => e.SenderUser);
        Query.Include(e => e.ReceiverUser);
        Query.Select(x => new ReviewAdminDTO
        {
            Id = x.Id,
            SenderUser = new UserDTO
            {
                Id = x.SenderUser.Id,
                FullName = x.SenderUser.FullName,
                Email = x.SenderUser.Email
            },
            ReceiverUser = new UserDTO
            {
                Id = x.ReceiverUser.Id,
                FullName = x.ReceiverUser.FullName,
                Email = x.ReceiverUser.Email
            },
            Content = x.Content,
            Rating = x.Rating
        });

        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }
    
    public ReviewAdminProjectionSpec(Guid id) : this() => Query.Where(e => e.Id == id);
    
    public ReviewAdminProjectionSpec(string? search) : this(true) 
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchExpr = $"%{search.Trim().Replace(" ", "%")}%";

            Query.Where(r =>
                EF.Functions.ILike(r.SenderUser.Id.ToString(), searchExpr) ||
                EF.Functions.ILike(r.ReceiverUser.Id.ToString(), searchExpr) ||
                EF.Functions.ILike(r.Content, searchExpr) ||
                EF.Functions.ILike(r.Rating.ToString(), searchExpr)
            );
        }
    }
}