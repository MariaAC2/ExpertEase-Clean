using System.Diagnostics;
using System.Net;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
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
using ExpertEase.Infrastructure.Repositories;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Services;

public class ConversationService(IRepository<WebAppDatabaseContext> repository, 
    IFirestoreRepository firestoreRepository, 
    IMessageService messageService,
    IMessageUpdateQueue messageUpdateQueue): IConversationService
{
    public async Task AddConversation(Conversation conversation, CancellationToken cancellationToken = default)
    {
        var firestoreDto = new FirestoreConversationDTO
        {
            Id = conversation.Id.ToString(),
            ParticipantIds = conversation.ParticipantIds.Select(id => id.ToString()).ToList(),
            Participants = conversation.ParticipantIds[0].ToString() + "_" + conversation.ParticipantIds[1].ToString(),
            RequestId = conversation.RequestId.ToString(),
            CreatedAt = Timestamp.FromDateTime(conversation.CreatedAt.ToUniversalTime())
        };

        await firestoreRepository.AddAsync("conversations", firestoreDto, cancellationToken);
    }

    public async Task<Conversation?> GetConversation(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var dto = await firestoreRepository.GetAsync<FirestoreConversationDTO>("conversations", conversationId.ToString(), cancellationToken);
        if (dto == null) return null;

        return new Conversation
        {
            Id = Guid.Parse(dto.Id),
            ParticipantIds = dto.ParticipantIds.Select(Guid.Parse).ToList(),
            RequestId = dto.RequestId,
            CreatedAt =  dto.CreatedAt.ToDateTime()
        };
    }
    public async Task UpdateConversationRequestId(Guid conversationId, Guid requestId, CancellationToken cancellationToken = default)
    {
        var dto = await firestoreRepository.GetAsync<FirestoreConversationDTO>("conversations", conversationId.ToString(), cancellationToken);
        if (dto == null) return;

        dto.RequestId = requestId.ToString();
        await firestoreRepository.UpdateAsync("conversations", dto, cancellationToken);
    }
    public async Task<ServiceResponse<ConversationDTO>> GetConversationByUsers(Guid currentUserId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // Fetch current user
        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);
        if (user is { Role: UserRoleEnum.Admin })
        {
            return ServiceResponse.CreateErrorResponse<ConversationDTO>(CommonErrors.NotAllowed);
        }
        
        var conversationKey = user.Role == UserRoleEnum.Client
            ? $"{currentUserId}_{userId}"
            : $"{userId}_{currentUserId}";
        
        // Fetch conversation
        var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
            "conversations",
            collection => collection.WhereEqualTo("Participants", conversationKey),
            cancellationToken);
        
        if (conversation == null)
        {
            return ServiceResponse.CreateErrorResponse<ConversationDTO>(
                new ErrorMessage(HttpStatusCode.NotFound, "Conversation not found", ErrorCodes.EntityNotFound));
        }
        
        var conversationId = Guid.Parse(conversation.Id);
        
        // Parallel fetching: requests, messages, other user
        var requests = await repository.ListAsync(new RequestConversationProjectionSpec(conversationId), cancellationToken);
        var messages = await messageService.GetMessagesBetweenUsers(conversationId, cancellationToken);
        
        var isClient = conversation.ClientData.UserId == currentUserId.ToString();
        var other = isClient ? conversation.SpecialistData : conversation.ClientData;
        
        // Handle unread messages
        var unreadMessages = messages
            .Where(m => !m.IsRead && m.SenderId != currentUserId)
            .ToList();
        
        foreach (var message in unreadMessages)
        {
            messageUpdateQueue.Enqueue(message.Id.ToString());
        }
        
        // Update unread count only if necessary
        if (conversation.UnreadCounts != null &&
            conversation.UnreadCounts.TryGetValue(currentUserId.ToString(), out var unreadCount) &&
            unreadCount > 0)
        {
            conversation.UnreadCounts[currentUserId.ToString()] = 0;
            await firestoreRepository.UpdateAsync("conversations", conversation, cancellationToken);
        }
        
        var result = new ConversationDTO
        {
            ConversationId = conversationId,
            UserId = Guid.Parse(other.UserId),
            UserFullName = other.UserFullName,
            UserProfilePictureUrl = other.UserProfilePictureUrl,
            Requests = requests,
            Messages = messages
        };

        stopwatch.Stop();
        Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");

        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse<List<UserConversationDTO>>> GetConversationsByUsers(Guid currentUserId, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);
        
        if (user.Role == UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse<List<UserConversationDTO>>(CommonErrors.NotAllowed);
        }
        
        var userIdString = currentUserId.ToString();

        var conversations = await firestoreRepository.ListAsync<FirestoreConversationDTO>(
            "conversations",
            collection => collection
                .WhereArrayContains("ParticipantIds", currentUserId.ToString())
                .OrderByDescending("LastMessageAt")
                .Limit(20),
            cancellationToken);

        var result = conversations.Select(c =>
        {
            var isClient = c.ClientData.UserId == currentUserId.ToString();
            var other = isClient ? c.SpecialistData : c.ClientData;
        
            return new UserConversationDTO
            {
                UserId = other.UserId,
                UserFullName = other.UserFullName,
                UserProfilePictureUrl = other.UserProfilePictureUrl
            };
        }).ToList();
        
        stopwatch.Stop();
        Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
        return ServiceResponse.CreateSuccessResponse(result);
    }
}