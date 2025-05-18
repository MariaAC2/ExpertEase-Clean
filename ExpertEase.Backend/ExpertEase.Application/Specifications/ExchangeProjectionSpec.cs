using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class ExchangeUserProjectionSpec : Specification<Request, UserExchangeDTO>
{
    public ExchangeUserProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Include(r => r.ReceiverUser)
            .Include(r => r.SenderUser)
            .OrderBy(r => r.ReceiverUserId);
        Query.Select(r => new UserExchangeDTO
        {
            Id = r.ReceiverUserId,
            FirstName = r.ReceiverUser.FirstName,
            LastName = r.ReceiverUser.LastName,
            Requests = new List<RequestDTO> {
                new RequestDTO
                {
                    Id = r.Id,
                    RequestedStartDate = r.RequestedStartDate,
                    Description = r.Description,
                    Status = r.Status,
                    SenderContactInfo = 
                        r.Status != StatusEnum.Rejected && r.Status != StatusEnum.Pending
                            ? new ContactInfoDTO
                            {
                                PhoneNumber = r.PhoneNumber,
                                Address = r.Address
                            }
                            : null,
                    Replies = r.Replies.Select(reply => new ReplyDTO
                    {
                        StartDate = reply.StartDate,
                        EndDate = reply.EndDate,
                        Price = reply.Price,
                        Status = reply.Status,
                    }).ToList()
                }
            }
        });
        
        if (orderByCreatedAt)
        {
            Query.OrderByDescending(r => r.CreatedAt);
        }
    }
    
    public ExchangeUserProjectionSpec(Guid id) : this() => Query.Where(e => e.ReceiverUserId == id);

    public ExchangeUserProjectionSpec(string? search) : this(true)
    {if (!string.IsNullOrWhiteSpace(search))
        {
            var searchExpr = $"%{search.Trim().Replace(" ", "%")}%";

            Query.Where(r =>
                EF.Functions.ILike(r.ReceiverUser.FirstName, searchExpr) ||
                EF.Functions.ILike(r.ReceiverUser.LastName, searchExpr)
            );
        }
    }
}

public class ExchangeSpecialistProjectionSpec : Specification<Request, UserExchangeDTO>
{
    public ExchangeSpecialistProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Include(r => r.ReceiverUser)
            .Include(r => r.SenderUser)
            .OrderBy(r => r.SenderUserId);
        Query.Select(r => new UserExchangeDTO
        {
            Id = r.SenderUserId,
            FirstName = r.SenderUser.FirstName,
            LastName = r.SenderUser.LastName,
            Requests = new List<RequestDTO> {
                new RequestDTO
                {
                    Id = r.Id,
                    RequestedStartDate = r.RequestedStartDate,
                    Description = r.Description,
                    Status = r.Status,
                    SenderContactInfo = 
                        r.Status != StatusEnum.Rejected && r.Status != StatusEnum.Pending
                            ? new ContactInfoDTO
                            {
                                PhoneNumber = r.PhoneNumber,
                                Address = r.Address
                            }
                            : null,
                    Replies = r.Replies.Select(reply => new ReplyDTO
                    {
                        StartDate = reply.StartDate,
                        EndDate = reply.EndDate,
                        Price = reply.Price,
                        Status = reply.Status,
                    }).ToList()
                }
            }
        });
        
        if (orderByCreatedAt)
        {
            Query.OrderByDescending(r => r.CreatedAt);
        }
    }
    
    public ExchangeSpecialistProjectionSpec(Guid id) : this() => Query.Where(e => e.SenderUserId == id);
    
    public ExchangeSpecialistProjectionSpec(string? search) : this(true)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchExpr = $"%{search.Trim().Replace(" ", "%")}%";

            Query.Where(r =>
                EF.Functions.ILike(r.SenderUser.FirstName, searchExpr) ||
                EF.Functions.ILike(r.SenderUser.LastName, searchExpr)
            );
        }
    }
}
