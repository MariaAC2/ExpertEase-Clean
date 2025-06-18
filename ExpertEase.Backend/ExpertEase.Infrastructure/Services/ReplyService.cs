using System.Net;
using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
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
using ExpertEase.Infrastructure.Firebase.FirestoreRepository;
using ExpertEase.Infrastructure.Firestore.FirestoreDTOs;
using ExpertEase.Infrastructure.Repositories;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Services;

public class ReplyService(IRepository<WebAppDatabaseContext> repository, 
    IServiceTaskService serviceTaskService,
    IPaymentService paymentService,
    IFirestoreRepository firestoreRepository,
    IConversationService conversationService,
    IConversationNotifier notificationService) : IReplyService
{
    public async Task<ServiceResponse> AddReply(Guid requestId, ReplyAddDTO reply, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser?.Role != UserRoleEnum.Specialist)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only specialists can create replies", ErrorCodes.CannotAdd));

        var request = await repository.GetAsync(new RequestSpec(requestId), cancellationToken);
        if (request == null || request.Status is StatusEnum.Failed or StatusEnum.Completed)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Cannot reply to this request", ErrorCodes.CannotAdd));

        if (request.Status != StatusEnum.Accepted)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest, "Request must be accepted before replying", ErrorCodes.CannotAdd));

        if (requestingUser.Id != request.ReceiverUserId)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Not the recipient of this request", ErrorCodes.CannotAdd));

        if (request.Replies.Count() > 5)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Exceeded reply limit", ErrorCodes.CannotAdd));

        if (reply.StartDate != null)
            reply.StartDate = reply.StartDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(reply.StartDate.Value, DateTimeKind.Utc)
                : reply.StartDate.Value.ToUniversalTime();

        reply.EndDate = reply.EndDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(reply.EndDate, DateTimeKind.Utc)
            : reply.EndDate.ToUniversalTime();

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
            var lastReply = request.Replies.OrderByDescending(r => r.CreatedAt).First();
            if (lastReply.Status != StatusEnum.Rejected)
                return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Cannot reply unless last reply was rejected", ErrorCodes.CannotAdd));

            if (lastReply.StartDate == newReply.StartDate && lastReply.EndDate == newReply.EndDate && lastReply.Price == newReply.Price)
                return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Conflict, "Duplicate reply", ErrorCodes.EntityAlreadyExists));
        }

        request.Replies.Add(newReply);
        await repository.AddAsync(newReply, cancellationToken);

        var conversationKey = $"{request.SenderUserId}_{request.ReceiverUserId}";
        var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
            "conversations",
            q => q.WhereEqualTo("Participants", conversationKey),
            cancellationToken);

        if (conversation == null) return ServiceResponse.CreateErrorResponse(new ErrorMessage(HttpStatusCode.NotFound, "Conversation not found", ErrorCodes.EntityNotFound));
        var firestoreReply = new FirestoreConversationItemDTO
        {
            Id = newReply.Id.ToString(),
            ConversationId = conversation.Id,
            SenderId = requestingUser.Id.ToString(),
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
            Type = "reply",
            Data = new Dictionary<string, object>
            {
                { "StartDate", Timestamp.FromDateTime(newReply.StartDate) },
                { "EndDate", Timestamp.FromDateTime(newReply.EndDate) },
                { "Price", (double)newReply.Price },
                { "Status", "Pending" },
                { "RequestId", requestId.ToString() }
            }
        };

        await firestoreRepository.AddAsync("conversationElements", firestoreReply, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse<ReplyPaymentDetailsDTO>> GetReply(Guid replyId, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new ReplyPaymentProjectionSpec(replyId), cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<ReplyPaymentDetailsDTO>(CommonErrors.EntityNotFound);
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

        if (entity.Status is not StatusEnum.Pending)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "Only pending replies can be updated", ErrorCodes.CannotUpdate));
        }

        // Store original data for notifications
        var specialistId = entity.Request.ReceiverUserId; // The specialist who sent the reply
        var clientId = requestingUser.Id; // The client performing the action

        switch (reply.Status)
        {
            // Handle different status updates
            case StatusEnum.Cancelled:
                entity.Status = StatusEnum.Cancelled;
                await repository.UpdateAsync(entity, cancellationToken);
            
                // 🆕 Update Firestore
                await UpdateFirestoreReplyStatus(entity.Id, StatusEnum.Cancelled, cancellationToken);
                
                // 🆕 Send notification
                await SendReplyCancelledNotification(entity, specialistId, cancellationToken);
                break;
                
            case StatusEnum.Rejected:
            {
                entity.Status = StatusEnum.Rejected;
                await repository.UpdateAsync(entity, cancellationToken);
            
                // 🆕 Update Firestore
                await UpdateFirestoreReplyStatus(entity.Id, StatusEnum.Rejected, cancellationToken);
                
                // 🆕 Send notification
                await SendReplyRejectedNotification(entity, specialistId, cancellationToken);
            
                var request = await repository.GetAsync(new RequestSpec(entity.RequestId), cancellationToken);
            
                if (request == null)
                    return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "Request not found", ErrorCodes.EntityNotFound));
            
                var activeReplies = request.Replies
                    .Where(r => r.Status != StatusEnum.Cancelled)
                    .ToList();

                if (activeReplies.Count is 0 or not 5) return ServiceResponse.CreateErrorResponse(new  (HttpStatusCode.Forbidden, "Replies not found", ErrorCodes.CannotUpdate));
                request.Status = StatusEnum.Failed;
                await repository.UpdateAsync(request, cancellationToken);
                break;
            }
            case StatusEnum.Accepted:
            {
                entity.Status = StatusEnum.Accepted;
                await repository.UpdateAsync(entity, cancellationToken);

                // Update Firestore
                await UpdateFirestoreReplyStatus(entity.Id, StatusEnum.Accepted, cancellationToken);
    
                // Send notification
                await SendReplyAcceptedNotification(entity, specialistId, cancellationToken);

                var request = await repository.GetAsync(new RequestSpec(entity.RequestId), cancellationToken);
                        
                if (request == null)
                    return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "Request not found", ErrorCodes.EntityNotFound));
    
                request.Status = StatusEnum.Completed;
                await repository.UpdateAsync(request, cancellationToken);

                // 🔄 ONLY CREATE PAYMENT INTENT - NOT SERVICE TASK YET
                try
                {
                    var specialistAccountId = await repository.GetAsync(new StripeAccountIdProjectionSpec(request.ReceiverUserId), cancellationToken);
                    if (string.IsNullOrEmpty(specialistAccountId))
                    {
                        return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest, "Specialist has no connected Stripe account", ErrorCodes.Invalid));
                    }

                    // Create payment intent for the frontend payment flow
                    var payment = new PaymentAddDTO
                    {
                        ReplyId = entity.Id, // Link to reply instead of service task
                        StripeAccountId = specialistAccountId,
                        Amount = entity.Price,
                    };
    
                    var paymentResponse = await paymentService.AddPayment(payment, cancellationToken);
    
                    if (paymentResponse.Error != null)
                    {
                        return ServiceResponse.CreateErrorResponse(paymentResponse.Error);
                    }

                    return ServiceResponse.CreateSuccessResponse();
                }
                catch (Exception ex)
                {
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.InternalServerError, $"Payment creation failed: {ex.Message}", ErrorCodes.TransactionFailed));
                }
            }
            case StatusEnum.Pending:
            case StatusEnum.Completed:
            case StatusEnum.Failed:
            default:
                return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                    "Other types of status codes not permitted", ErrorCodes.CannotUpdate));
        }
        return ServiceResponse.CreateSuccessResponse();
    }

    // 🆕 Private methods for sending notifications
    private async Task SendReplyAcceptedNotification(Reply reply, Guid specialistId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                ReplyId = reply.Id,
                RequestId = reply.RequestId,
                SpecialistId = specialistId,
                Status = "Accepted",
                StartDate = reply.StartDate,
                EndDate = reply.EndDate,
                Price = reply.Price,
                UpdatedAt = DateTime.UtcNow,
                Message = "Great news! Your service offer has been accepted and payment processing has begun."
            };

            await notificationService.NotifyReplyAccepted(specialistId, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send reply accepted notification for reply {reply.Id}: {ex.Message}");
        }
    }

    private async Task SendReplyRejectedNotification(Reply reply, Guid specialistId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                ReplyId = reply.Id,
                RequestId = reply.RequestId,
                SpecialistId = specialistId,
                Status = "Rejected",
                StartDate = reply.StartDate,
                EndDate = reply.EndDate,
                Price = reply.Price,
                UpdatedAt = DateTime.UtcNow,
                Message = "Your service offer has been declined by the client."
            };

            await notificationService.NotifyReplyRejected(specialistId, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send reply rejected notification for reply {reply.Id}: {ex.Message}");
        }
    }

    private async Task SendReplyCancelledNotification(Reply reply, Guid specialistId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                ReplyId = reply.Id,
                reply.RequestId,
                SpecialistId = specialistId,
                Status = "Cancelled",
                reply.StartDate,
                reply.EndDate,
                reply.Price,
                UpdatedAt = DateTime.UtcNow,
                Message = "A service offer has been cancelled."
            };

            await notificationService.NotifyReplyCancelled(specialistId, payload);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send reply cancelled notification for reply {reply.Id}: {ex.Message}");
        }
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

        if (entity.Status is not StatusEnum.Pending)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "Only pending replies can be updated", ErrorCodes.CannotUpdate));
        }
        
        // Update PostgreSQL
        entity.StartDate = reply.StartDate ?? entity.StartDate;
        entity.EndDate = reply.EndDate ?? entity.EndDate;
        entity.Price = reply.Price ?? entity.Price;
        
        await repository.UpdateAsync(entity, cancellationToken);

        // 🆕 Update Firestore conversation item data
        await UpdateFirestoreReplyData(entity, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    /// <summary>
    /// Update the status of a reply in Firestore conversation elements
    /// </summary>
    private async Task UpdateFirestoreReplyStatus(Guid replyId, StatusEnum newStatus, CancellationToken cancellationToken)
    {
        try
        {
            var firestoreItem = await firestoreRepository.GetAsync<FirestoreConversationItemDTO>(
                "conversationElements", 
                replyId.ToString(), 
                cancellationToken);

            if (firestoreItem != null)
            {
                firestoreItem.Data["Status"] = newStatus.ToString();
                await firestoreRepository.UpdateAsync("conversationElements", firestoreItem, cancellationToken);
            }
            else
            {
                Console.WriteLine($"Warning: Firestore conversation item not found for reply ID {replyId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating Firestore reply status: {ex.Message}");
        }
    }

    /// <summary>
    /// Update reply data fields in Firestore conversation elements
    /// </summary>
    private async Task UpdateFirestoreReplyData(Reply entity, CancellationToken cancellationToken)
    {
        try
        {
            var firestoreItem = await firestoreRepository.GetAsync<FirestoreConversationItemDTO>(
                "conversationElements", 
                entity.Id.ToString(), 
                cancellationToken);

            if (firestoreItem != null)
            {
                firestoreItem.Data["StartDate"] = Timestamp.FromDateTime(entity.StartDate);
                firestoreItem.Data["EndDate"] = Timestamp.FromDateTime(entity.EndDate);
                firestoreItem.Data["Price"] = entity.Price;
                
                await firestoreRepository.UpdateAsync("conversationElements", firestoreItem, cancellationToken);
            }
            else
            {
                Console.WriteLine($"Warning: Firestore conversation item not found for reply ID {entity.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating Firestore reply data: {ex.Message}");
        }
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