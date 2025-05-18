﻿using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class RequestProjectionSpec : Specification<Request, RequestDTO>
{
    public RequestProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Include(r => r.SenderUser);
        Query.Include(r => r.ReceiverUser);
        Query.Include(r => r.Replies);
        Query.Select(e => new RequestDTO
        {
            Id = e.Id,
            RequestedStartDate = e.RequestedStartDate,
            Description = e.Description,
            Status = e.Status,
            SenderContactInfo = 
                e.Status != StatusEnum.Rejected && e.Status != StatusEnum.Pending
                    ? new ContactInfoDTO
                    {
                        PhoneNumber = e.PhoneNumber,
                        Address = e.Address
                    }
                    : null,
            Replies = e.Replies.Select(r => new ReplyDTO
            {
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Price = r.Price,
                Status = r.Status,
            }).ToList()
        });

        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }
    
    public RequestProjectionSpec(Guid id) : this() => Query.Where(e => e.Id == id);
    
    public RequestProjectionSpec(string? search) : this(true) // This constructor will call the first declared constructor with 'true' as the parameter. 
    {
        Query.Include(r => r.SenderUser);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchExpr = $"%{search.Trim().Replace(" ", "%")}%";

            Query.Where(r =>
                    EF.Functions.ILike(r.Description, searchExpr) ||
                    EF.Functions.ILike(r.Status.ToString(), searchExpr) ||
                    EF.Functions.ILike(r.SenderUser.FirstName, searchExpr) ||
                    EF.Functions.ILike(r.SenderUser.LastName, searchExpr) ||
                    EF.Functions.ILike(r.SenderUser.Email, searchExpr) ||
                    EF.Functions.ILike(r.PhoneNumber, searchExpr) ||
                    EF.Functions.ILike(r.Address, searchExpr)
            );
        }
    }
}

public class RequestUserProjectionSpec : RequestProjectionSpec
{
    public RequestUserProjectionSpec(Guid id, Guid userId) : base(id) => Query.Where(e => e.Id == id && e.SenderUserId == userId);

    public RequestUserProjectionSpec(string? search, Guid userId) : base(search)
    {
        Query.Include(r => r.SenderUser);
        Query.Where(r=> r.SenderUserId == userId);
    }
}

public class RequestSpecialistProjectionSpec : RequestProjectionSpec
{
    public RequestSpecialistProjectionSpec(Guid id, Guid userId) : base(id) => Query.Where(e =>  e.Id == id && e.ReceiverUserId == userId);

    public RequestSpecialistProjectionSpec(string? search, Guid userId) : base(search)
    {
        Query.Include(r => r.ReceiverUser);
        Query.Where(r => r.ReceiverUserId == userId);
    }
}