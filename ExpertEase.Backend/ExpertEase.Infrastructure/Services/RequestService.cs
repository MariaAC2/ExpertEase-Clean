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
using SystemException = System.SystemException;

namespace ExpertEase.Infrastructure.Services;

public class RequestService(IRepository<WebAppDatabaseContext> repository, 
    IFirestoreRepository firestoreRepository, 
    IConversationService conversationService,
    IConversationNotifier notificationService) : IRequestService
{
    public async Task<ServiceResponse> AddRequest(RequestAddDTO request, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));

        if (requestingUser.Role != UserRoleEnum.Client)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only users can create requests", ErrorCodes.CannotAdd));

        var sender = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
        var receiver = await repository.GetAsync(new UserSpec(request.ReceiverUserId), cancellationToken);

        if (sender == null || receiver is not { Role: UserRoleEnum.Specialist })
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
            Id = requestEntity.Id.ToString(),
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
        
        return result == null ? ServiceResponse.CreateErrorResponse<RequestDTO>(new (HttpStatusCode.NotFound, "Request not found", ErrorCodes.EntityNotFound)) : ServiceResponse.CreateSuccessResponse(result);
    }

    public async Task<ServiceResponse<PagedResponse<RequestDTO>>> GetRequests(Specification<Request, RequestDTO> spec,
        PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, spec, cancellationToken); // Use the specification and pagination API to get only some entities from the database.

        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse<int>> GetRequestCount(CancellationToken cancellationToken = default) => 
        ServiceResponse.CreateSuccessResponse(await repository.GetCountAsync<Request>(cancellationToken));
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
        
        // Store original data for notification payload
        var originalSenderId = entity.SenderUserId;
        var specialistId = requestingUser?.Id ?? entity.ReceiverUserId; // The one performing the action
        
        // Update PostgreSQL
        entity.Status = request.Status;
        if (entity.Status == StatusEnum.Rejected)
        {
            entity.RejectedAt = DateTime.UtcNow;
        }
        
        await repository.UpdateAsync(entity, cancellationToken);

        // 🆕 Update Firestore conversation item
        await UpdateFirestoreRequestStatus(entity.Id, request.Status, cancellationToken);
        
        // 🆕 Send SignalR notifications based on status
        await SendRequestStatusNotification(entity, originalSenderId, specialistId, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    private async Task SendRequestStatusNotification(
        Request entity, 
        Guid originalSenderId, 
        Guid? specialistId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create notification payload with relevant request data
            var payload = new
            {
                RequestId = entity.Id,
                SenderId = originalSenderId,
                SpecialistId = specialistId,
                Status = entity.Status.ToString(),
                entity.Description,
                entity.RequestedStartDate,
                entity.PhoneNumber,
                entity.Address,
                UpdatedAt = DateTime.UtcNow,
                // Add any other relevant data for the frontend
                Message = GetStatusUpdateMessage(entity.Status)
            };

            switch (entity.Status)
            {
                case StatusEnum.Accepted:
                    // Notify the original requester (client) that their request was accepted
                    await notificationService.NotifyRequestAccepted(originalSenderId, payload);
                    break;
                    
                case StatusEnum.Rejected:
                    // Notify the original requester (client) that their request was rejected
                    await notificationService.NotifyRequestRejected(originalSenderId, payload);
                    break;
                    
                case StatusEnum.Cancelled:
                    // Notify the specialist that the request was cancelled by the client
                    if (specialistId.HasValue && specialistId != originalSenderId)
                    {
                        await notificationService.NotifyRequestCancelled(specialistId.Value, payload);
                    }
                    break;
            }
        }
        catch (SystemException ex)
        {
            // Log the error but don't fail the main operation
            Console.WriteLine($"Error sending request status notification: {ex.Message}");
        }
    }

    private static string GetStatusUpdateMessage(StatusEnum status)
    {
        return status switch
        {
            StatusEnum.Accepted => "Your service request has been accepted!",
            StatusEnum.Rejected => "Your service request has been declined.",
            StatusEnum.Cancelled => "The service request has been cancelled.",
            _ => "Service request status updated."
        };
    }

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
        
        // Update PostgreSQL
        entity.RequestedStartDate = request.RequestedStartDate ?? entity.RequestedStartDate;
        entity.PhoneNumber = request.PhoneNumber ?? entity.PhoneNumber;
        entity.Address = request.Address ?? entity.Address;
        entity.Description = request.Description ?? entity.Description;
        
        await repository.UpdateAsync(entity, cancellationToken);

        // 🆕 Update Firestore conversation item data
        await UpdateFirestoreRequestData(entity, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }
        
    /// <summary>
    /// Update the status of a request in Firestore conversation elements
    /// </summary>
    private async Task UpdateFirestoreRequestStatus(Guid requestId, StatusEnum newStatus, CancellationToken cancellationToken)
    {
        try
        {
            // Find the Firestore conversation item using the PostgreSQL request ID
            var firestoreItem = await firestoreRepository.GetAsync<FirestoreConversationItemDTO>(
                "conversationElements", 
                requestId.ToString(), 
                cancellationToken);

            if (firestoreItem != null)
            {
                // Update the status in the data dictionary
                firestoreItem.Data["Status"] = newStatus.ToString();
                
                // Update the Firestore document
                await firestoreRepository.UpdateAsync("conversationElements", firestoreItem, cancellationToken);
            }
            else
            {
                // Log warning if Firestore item not found
                Console.WriteLine($"Warning: Firestore conversation item not found for request ID {requestId}");
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the main operation
            Console.WriteLine($"Error updating Firestore request status: {ex.Message}");
        }
    }

    /// <summary>
    /// Update request data fields in Firestore conversation elements
    /// </summary>
    private async Task UpdateFirestoreRequestData(Request entity, CancellationToken cancellationToken)
    {
        try
        {
            var firestoreItem = await firestoreRepository.GetAsync<FirestoreConversationItemDTO>(
                "conversationElements", 
                entity.Id.ToString(), 
                cancellationToken);

            if (firestoreItem != null)
            {
                // Update the data fields
                firestoreItem.Data["RequestedStartDate"] = Timestamp.FromDateTime(entity.RequestedStartDate);
                firestoreItem.Data["PhoneNumber"] = entity.PhoneNumber;
                firestoreItem.Data["Address"] = entity.Address;
                firestoreItem.Data["Description"] = entity.Description;
                
                await firestoreRepository.UpdateAsync("conversationElements", firestoreItem, cancellationToken);
            }
            else
            {
                Console.WriteLine($"Warning: Firestore conversation item not found for request ID {entity.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating Firestore request data: {ex.Message}");
        }
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