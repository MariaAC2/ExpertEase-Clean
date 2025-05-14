using System.Net;
using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class RequestService(IRepository<WebAppDatabaseContext> repository) : IRequestService
{
    public async Task<ServiceResponse> AddRequest(RequestAddDTO request, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));
        }
        if (requestingUser.Role != UserRoleEnum.Client)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only users can create requests", ErrorCodes.CannotAdd));
        }
        
        var sender = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);

        if (sender == null)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.NotFound, "User not found", ErrorCodes.EntityNotFound));
        }

        if (sender.Id != requestingUser.Id)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the own user can create requests", ErrorCodes.CannotAdd));
        }

        if (sender.Requests.Any())
        {
            var lastReply = sender.Requests
                .OrderByDescending(r => r.CreatedAt)
                .First();

            // if (lastReply != null)
            // {
            //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Conflict, lastReply.ToString()));
            // }

            if (lastReply.Status != StatusEnum.Failed && lastReply.Status != StatusEnum.Completed)
            {
                return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "Cannot create request until last request is finalized", ErrorCodes.CannotAdd));
            }
        }
        
        var receiver = await repository.GetAsync(new UserSpec(request.ReceiverUserId), cancellationToken);
        
        if (receiver == null)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.NotFound, "User with this ID not found", ErrorCodes.EntityNotFound));
        }

        if (receiver.Role != UserRoleEnum.Specialist)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Requests are sent only to specialists!", ErrorCodes.CannotAdd));
        }
        
        var requestEntity = new Request
        {
            SenderUserId = requestingUser.Id,
            SenderUser = sender,
            ReceiverUserId = request.ReceiverUserId,
            ReceiverUser = receiver,
            RequestedStartDate = request.RequestedStartDate,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Description = request.Description,
            Status = StatusEnum.Pending
        };
        
        var existingRequest = await repository.GetAsync(new RequestSearchSpec(requestEntity), cancellationToken);
        
        if (existingRequest != null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Conflict, "Request already exists", ErrorCodes.CannotAdd));
        }
        
        await repository.AddAsync(requestEntity, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse<RequestDTO>> GetRequest(Specification<Request, RequestDTO> spec, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(spec, cancellationToken);
        
        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse<RequestDTO>(new (HttpStatusCode.NotFound, "Request not found", ErrorCodes.EntityNotFound));
        }

        return ServiceResponse.CreateSuccessResponse(result);
    }

    public async Task<ServiceResponse<PagedResponse<RequestDTO>>> GetRequests(Specification<Request, RequestDTO> spec,
        PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, spec, cancellationToken); // Use the specification and pagination API to get only some entities from the database.

        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse<int>> GetRequestCount(CancellationToken cancellationToken = default) => 
        ServiceResponse.CreateSuccessResponse(await repository.GetCountAsync<Request>(cancellationToken));

    public async Task<ServiceResponse> UpdateRequest(RequestUpdateDTO request, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Client && 
            (request.Description != null || request.PhoneNumber != null || request.Address != null || request.RequestedStartDate != null))
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Non users cannot update request info", ErrorCodes.CannotUpdate));
        }
        
        var entity = await repository.GetAsync(new RequestSpec(request.Id), cancellationToken);

        if (entity == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Request not found", ErrorCodes.EntityNotFound));
        }
        
        entity.RequestedStartDate = request.RequestedStartDate ?? entity.RequestedStartDate;
        entity.PhoneNumber = request.PhoneNumber ?? entity.PhoneNumber;
        entity.Address = request.Address ?? entity.Address;
        entity.Description = request.Description ?? entity.Description;
        
        await repository.UpdateAsync(entity, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }
    
        public async Task<ServiceResponse> UpdateRequestStatus(StatusUpdateDTO request, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role == UserRoleEnum.Client && request.Status != StatusEnum.Cancelled)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Users cannot accept or reject request", ErrorCodes.CannotUpdate));
        }
        
        var entity = await repository.GetAsync(new RequestSpec(request.Id), cancellationToken);

        if (entity == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Request not found", ErrorCodes.EntityNotFound));
        }
        
        if (entity.Status != StatusEnum.Pending)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Request is already processed", ErrorCodes.CannotUpdate));
        }
        
        entity.Status = request.Status;
        
        await repository.UpdateAsync(entity, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse> DeleteRequest(Guid id, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && requestingUser.Id != id) // Verify who can add the user, you can change this however you se fit.
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin or the own user can delete the user!", ErrorCodes.CannotDelete));
        }

        await repository.DeleteAsync<Request>(id, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }
}