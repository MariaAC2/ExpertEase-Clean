using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class ExchangeUserProjectionSpec : Specification<Request, UserExchangeDTO>
{
    
    public ExchangeUserProjectionSpec(Guid receiverUserId, bool orderByCreatedAt = false)
{
    Query.Include(r => r.ReceiverUser)
         .Include(r => r.SenderUser)
         .Include(r => r.Replies)
         .OrderBy(r => r.SenderUserId);

    Query.Where(e => e.ReceiverUserId == receiverUserId);

    Query.Select(r => new UserExchangeDTO
    {
        Id = r.SenderUserId,
        FullName = r.SenderUser.FullName,
        Requests = new List<RequestDTO> {
            new RequestDTO
            {
                Id = r.Id,
                SenderUserId = r.SenderUserId,
                ReceiverUserId = r.ReceiverUserId,
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
                Replies = r.Replies
                    .Select((reply, index) => new ReplyDTO
                    {
                        Id = reply.Id,
                        StartDate = reply.StartDate,
                        EndDate = reply.EndDate,
                        Price = reply.Price,
                        Status = reply.Status,
                        ServiceTask = index == 0 && reply.ServiceTask != null ? new ServiceTaskDTO
                        {
                            Id = reply.ServiceTask.Id,
                            ReplyId = reply.ServiceTask.ReplyId,
                            StartDate = reply.ServiceTask.StartDate,
                            EndDate = reply.ServiceTask.EndDate,
                            Description = reply.ServiceTask.Description,
                            Address = reply.ServiceTask.Address,
                            Price = reply.ServiceTask.Price,
                            Status = reply.ServiceTask.Status,
                        } : null
                    }).ToList()
            }
        }
    });

    if (orderByCreatedAt)
    {
        Query.OrderByDescending(r => r.CreatedAt);
    }
}

    // public ExchangeUserProjectionSpec(Guid receiverUserId, bool orderByCreatedAt = false)
    // {
    //     Query.Include(r => r.ReceiverUser)
    //         .Include(r => r.SenderUser)
    //         .OrderBy(r => r.SenderUserId);
    //     Query.Where(e => e.ReceiverUserId == receiverUserId);
    //     Query.Select(r => new UserExchangeDTO
    //     {
    //         Id = r.SenderUserId,
    //         FullName = r.SenderUser.FullName,
    //         Requests = new List<RequestDTO> {
    //             new RequestDTO
    //             {
    //                 Id = r.Id,
    //                 SenderUserId = r.SenderUserId,
    //                 ReceiverUserId = r.ReceiverUserId,
    //                 RequestedStartDate = r.RequestedStartDate,
    //                 Description = r.Description,
    //                 Status = r.Status,
    //                 SenderContactInfo = 
    //                     r.Status != StatusEnum.Rejected && r.Status != StatusEnum.Pending
    //                         ? new ContactInfoDTO
    //                         {
    //                             PhoneNumber = r.PhoneNumber,
    //                             Address = r.Address
    //                         }
    //                         : null,
    //                 Replies = r.Replies.Select(reply => new ReplyDTO
    //                 {
    //                     StartDate = reply.StartDate,
    //                     EndDate = reply.EndDate,
    //                     Price = reply.Price,
    //                     Status = reply.Status,
    //                 }).ToList()
    //             }
    //         }
    //     });
    //     
    //     if (orderByCreatedAt)
    //     {
    //         Query.OrderByDescending(r => r.CreatedAt);
    //     }
    // }
    
    public ExchangeUserProjectionSpec(Guid senderUserId, Guid receiverUserId) : this(receiverUserId) => Query.Where(e => e.SenderUserId == senderUserId);

    public ExchangeUserProjectionSpec(string? search, Guid userId) : this(userId, true)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchExpr = $"%{search.Trim().Replace(" ", "%")}%";

            Query.Where(r =>
                EF.Functions.ILike(r.ReceiverUser.FullName, searchExpr)
            );
        }
    }
}