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
using ExpertEase.Infrastructure.Firestore.FirestoreDTOs;
using ExpertEase.Infrastructure.Repositories;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Services;

public class RequestService(IRepository<WebAppDatabaseContext> repository, IFirestoreRepository firestoreRepository, IConversationService conversationService) : IRequestService
{
    public async Task<ServiceResponse> AddRequest(RequestAddDTO request, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));

        if (requestingUser.Role != UserRoleEnum.Client)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only users can create requests", ErrorCodes.CannotAdd));

        var sender = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
        var receiver = await repository.GetAsync(new UserSpec(request.ReceiverUserId), cancellationToken);

        if (sender == null || receiver == null || receiver.Role != UserRoleEnum.Specialist)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Invalid sender or receiver", ErrorCodes.CannotAdd));

        var conversationKey = $"{requestingUser.Id}_{request.ReceiverUserId}";
        var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
            "conversations",
            q => q.WhereEqualTo("Participants", conversationKey),
            cancellationToken);

        if (conversation == null)
        {
            conversation = new FirestoreConversationDTO
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
                    UserId = receiver.Id.ToString(),
                    UserFullName = receiver.FullName,
                    UserProfilePictureUrl = receiver.ProfilePictureUrl
                },
                CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            await firestoreRepository.AddAsync("conversations", conversation, cancellationToken);
        }
        else
        {
            var existingRequests = await repository.ListAsync(new RequestConversationSpec(conversation.Id), cancellationToken);
            var lastRequest = existingRequests.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

            if (lastRequest != null && lastRequest.Status != StatusEnum.Failed && lastRequest.Status != StatusEnum.Completed)
                return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Cannot create request until last is finalized", ErrorCodes.CannotAdd));
        }

        request.RequestedStartDate = request.RequestedStartDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.RequestedStartDate, DateTimeKind.Utc)
            : request.RequestedStartDate.ToUniversalTime();

        var requestEntity = new Request
        {
            SenderUserId = sender.Id,
            SenderUser = sender,
            ReceiverUserId = receiver.Id,
            ReceiverUser = receiver,
            RequestedStartDate = request.RequestedStartDate,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Description = request.Description,
            Status = StatusEnum.Pending,
            ConversationId = conversation.Id
        };

        await repository.AddAsync(requestEntity, cancellationToken);

        var firestoreRequest = new FirestoreConversationItemDTO
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conversation.Id,
            SenderId = sender.Id.ToString(),
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
            Type = "request",
            Data = new Dictionary<string, object>
            {
                { "RequestedStartDate", Timestamp.FromDateTime(request.RequestedStartDate) },
                { "PhoneNumber", request.PhoneNumber },
                { "Address", request.Address },
                { "Description", request.Description },
                { "Status", "Pending" }
            }
        };

        await firestoreRepository.AddAsync("conversationElements", firestoreRequest, cancellationToken);
        await conversationService.UpdateConversationRequestId(Guid.Parse(conversation.Id), requestEntity.Id, cancellationToken);

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