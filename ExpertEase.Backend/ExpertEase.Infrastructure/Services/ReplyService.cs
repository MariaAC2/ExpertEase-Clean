using System.Net;
using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class ReplyService(IRepository<WebAppDatabaseContext> repository, IServiceTaskService serviceTaskService) : IReplyService
{
    public async Task<ServiceResponse> AddReply(Guid requestId, ReplyAddDTO reply, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Specialist)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "Only specialists can create replies", ErrorCodes.CannotAdd));
        }
        
        var request = await repository.GetAsync(new RequestSpec(requestId), cancellationToken);
        
        if (request == null)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.NotFound, "Request not found", ErrorCodes.EntityNotFound));
        }

        if (request.Status != StatusEnum.Accepted)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.BadRequest, "Cannot reply to an unaccepted request", ErrorCodes.CannotAdd));
        }
        
        if (request.Status == StatusEnum.Failed || request.Status == StatusEnum.Confirmed)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "Cannot reply to a completed or failed request", ErrorCodes.CannotAdd));
        }
        
        if (requestingUser != null && requestingUser.Id != request.ReceiverUserId)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "Only the user that received the request can reply", ErrorCodes.CannotAdd));
        }
        
        if (request.Replies.Any())
        {
            var lastReply = request.Replies
                .OrderByDescending(r => r.CreatedAt)
                .First();
        
            if (lastReply.Status != StatusEnum.Rejected)
            {
                return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "Cannot create reply if last reply was not rejected", ErrorCodes.CannotAdd));
            }
        }

        if (request.Replies.Count() > 5)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Exceeded 5 replies per request",
                ErrorCodes.CannotAdd));
        }
        
        if (reply.StartDate != null)
        {
            if (reply.StartDate.Value.Kind == DateTimeKind.Unspecified)
            {
                reply.StartDate = DateTime.SpecifyKind(reply.StartDate.Value, DateTimeKind.Utc);
            }
            else
            {
                reply.StartDate = reply.StartDate.Value.ToUniversalTime();
            }
        }
        
        if (reply.EndDate.Kind == DateTimeKind.Unspecified)
        {
            reply.EndDate = DateTime.SpecifyKind(reply.EndDate, DateTimeKind.Utc);
        }
        else
        {
            reply.EndDate = reply.EndDate.ToUniversalTime();
        }

        var newReply = new Reply
        {
            RequestId = requestId,
            Request = request,
            Status = StatusEnum.Pending,
            StartDate = reply.StartDate ?? request.RequestedStartDate,
            EndDate = reply.EndDate,
            Price = reply.Price
        };
        
        if (request.Replies.Any())
        {
            var lastReply = request.Replies
                .OrderByDescending(r => r.CreatedAt)
                .First();
        
            bool isDuplicate =
                lastReply.StartDate == newReply.StartDate &&
                lastReply.EndDate == newReply.EndDate &&
                lastReply.Price == newReply.Price;
        
            if (isDuplicate)
            {
                return ServiceResponse.CreateErrorResponse(new(
                    HttpStatusCode.Conflict,
                    "Cannot add a reply identical to the last one.",
                    ErrorCodes.EntityAlreadyExists));
            }
        }
        
        request.Replies.Add(newReply);
        await repository.AddAsync(newReply, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse<ReplyDTO>> GetReply(Specification<Reply, ReplyDTO> spec, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(spec, cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<ReplyDTO>(CommonErrors.EntityNotFound);
    }

    public async Task<ServiceResponse<PagedResponse<ReplyDTO>>> GetReplies(Specification<Reply, ReplyDTO> spec, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, spec, cancellationToken);

        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse> UpdateReplyStatus(StatusUpdateDTO reply, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "Cannot add replies without being authenticated", ErrorCodes.CannotUpdate));
        }
        if (requestingUser.Role == UserRoleEnum.Specialist)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "Only user and admin can accept or reject replies", ErrorCodes.CannotUpdate));
        }
        
        var entity = await repository.GetAsync(new ReplySpec(reply.Id), cancellationToken);
        
        if (entity == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound,
                "Reply not found", ErrorCodes.EntityNotFound));
        }

        // if (entity.Status is not StatusEnum.Pending)
        // {
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
        //         "Only pending replies can be updated", ErrorCodes.CannotUpdate));
        // }
        
        if (reply.Status is StatusEnum.Rejected)
        {
            entity.Status = StatusEnum.Rejected;
            await repository.UpdateAsync(entity, cancellationToken);
            var request = await repository.GetAsync(new RequestSpec(entity.RequestId), cancellationToken);
            
            if (request == null)
                return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "Request not found", ErrorCodes.EntityNotFound));
            
            if (request.Replies.Any() && request.Replies.Count == 5)
            {
                request.Status = StatusEnum.Failed;
                await repository.UpdateAsync(request, cancellationToken);
            }
        } else if (reply.Status is StatusEnum.Accepted)
        {
            entity.Status = StatusEnum.Accepted;
            await repository.UpdateAsync(entity, cancellationToken);
            var request = await repository.GetAsync(new RequestSpec(entity.RequestId), cancellationToken);
                            
            if (request == null)
                return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "Request not found", ErrorCodes.EntityNotFound));
            
            if (request.Replies.Any())
            {
                request.Status = StatusEnum.Confirmed;
                await repository.UpdateAsync(request, cancellationToken);
                // create transfer transaction
                var transferResult = await serviceTaskService.AddServiceTask(entity, cancellationToken);

                if (!transferResult.IsOk)
                {
                    return transferResult;
                }
            }
        }
        else
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "Other types of status codes not permitted", ErrorCodes.CannotUpdate));
        }
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse> UpdateReply(ReplyUpdateDTO reply, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "Cannot add replies without being authenticated", ErrorCodes.CannotUpdate));
        }
    
        if (requestingUser.Role != UserRoleEnum.Specialist)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "Only specialist can modify service info", ErrorCodes.CannotUpdate));
        }
        
        var entity = await repository.GetAsync(new ReplySpec(reply.Id), cancellationToken);
        
        if (entity == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound,
                "Reply not found", ErrorCodes.EntityNotFound));
        }
    
        // if (entity.Status is not StatusEnum.Pending)
        // {
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
        //         "Only pending replies can be updated", ErrorCodes.CannotUpdate));
        // }
        
        entity.StartDate = reply.StartDate ?? entity.StartDate;
        entity.EndDate = reply.EndDate ?? entity.EndDate;
        entity.Price = reply.Price ?? entity.Price;
        
        await repository.UpdateAsync(entity, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse> DeleteReply(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin can delete replies!", ErrorCodes.CannotDelete));
        }

        await repository.DeleteAsync<Reply>(id, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }
}