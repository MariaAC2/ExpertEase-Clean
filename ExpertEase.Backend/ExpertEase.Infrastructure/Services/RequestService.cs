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
using ExpertEase.Infrastructure.Firebase.FirestoreRepository;
using ExpertEase.Infrastructure.Repositories;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Services;

public class RequestService(IRepository<WebAppDatabaseContext> repository, IFirestoreRepository firestoreRepository, IConversationService conversationService) : IRequestService
{
    public async Task<ServiceResponse> AddRequest(RequestAddDTO request, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));
        }
        if (requestingUser.Role != UserRoleEnum.Client)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only users can create requests", ErrorCodes.CannotAdd));
        }

        var sender = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
        if (sender == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this ID not found", ErrorCodes.EntityNotFound));
        }
        var receiver = await repository.GetAsync(new UserSpec(request.ReceiverUserId), cancellationToken);
        if (receiver == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this ID not found", ErrorCodes.EntityNotFound));
        }
        if (receiver.Role != UserRoleEnum.Specialist)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Requests are sent only to specialists!", ErrorCodes.CannotAdd));
        }
        
        var sortedIds = new[] { requestingUser.Id, request.ReceiverUserId }.OrderBy(id => id);
        var conversationKey = string.Join("_", sortedIds);
        
        var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
            "conversations",
            collection => collection.WhereEqualTo("ConversationKey", conversationKey),
            cancellationToken);

        if (conversation == null)
        {
            var newConversation = new FirestoreConversationDTO
            {
                Id = Guid.NewGuid().ToString(),
                ParticipantIds = [requestingUser.Id.ToString(), request.ReceiverUserId.ToString()],
                Participants = conversationKey,
                RequestId = "",
                ClientData = new FirestoreUserConversationDTO
                {
                    UserId = requestingUser.Id.ToString(),
                    UserFullName = requestingUser.FullName,
                    UserProfilePictureUrl = requestingUser.ProfilePictureUrl
                },
                SpecialistData = new FirestoreUserConversationDTO
                {
                    UserId = request.ReceiverUserId.ToString(),
                    UserFullName = receiver.FullName,
                    UserProfilePictureUrl = receiver.ProfilePictureUrl
                },
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow.ToUniversalTime())
            };
            await firestoreRepository.AddAsync("conversations", newConversation, cancellationToken);
            
            var conversationDTO = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
                "conversations",
                collection => collection.WhereEqualTo("Participants", conversationKey),
                cancellationToken);
            
            request.RequestedStartDate = request.RequestedStartDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(request.RequestedStartDate, DateTimeKind.Utc) 
                : request.RequestedStartDate.ToUniversalTime();
            
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
                Status = StatusEnum.Pending,
                ConversationId = conversationDTO.Id
            };
            await repository.AddAsync(requestEntity, cancellationToken);
            
            var request1 = await repository.GetAsync(new RequestSpec(requestEntity.Id), cancellationToken);
            if (request1 == null)
            {
                return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.InternalServerError, "Request creation failed", ErrorCodes.CannotAdd));
            }
            
            await conversationService.UpdateConversationRequestId(Guid.Parse(conversationDTO.Id), request1.Id, cancellationToken);
        }
        else
        {
            var requests = await repository.ListAsync(new RequestConversationSpec(conversation.Id), cancellationToken);
            
            if (requests.Count > 0)
            {
                var lastReply = requests.OrderByDescending(r => r.CreatedAt).First();
                if (lastReply.Status != StatusEnum.Failed && lastReply.Status != StatusEnum.Completed)
                {
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Cannot create request until last request is finalized", ErrorCodes.CannotAdd));
                }
            }

            request.RequestedStartDate = request.RequestedStartDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(request.RequestedStartDate, DateTimeKind.Utc) 
                : request.RequestedStartDate.ToUniversalTime();
            
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
                Status = StatusEnum.Pending,
                ConversationId = conversation.Id
            };
            await repository.AddAsync(requestEntity, cancellationToken);
        }

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
        if (requestingUser is { Role: UserRoleEnum.Client } && request.Status != StatusEnum.Cancelled)
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

        if (entity.Status == StatusEnum.Rejected)
        {
            entity.RejectedAt = DateTime.UtcNow;
        }
        
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