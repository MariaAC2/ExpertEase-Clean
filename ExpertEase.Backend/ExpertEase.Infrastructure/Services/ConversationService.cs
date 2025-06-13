using System.Diagnostics;
using System.Net;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
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

public class ConversationService(IRepository<WebAppDatabaseContext> repository, 
    IFirestoreRepository firestoreRepository, 
    IMessageService messageService,
    IMessageUpdateQueue messageUpdateQueue,
    IConversationNotifier notifier): IConversationService
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
    public async Task<ServiceResponse> AddConvElement(
        FirestoreConversationItemAddDTO firestoreMessage,
        Guid conversationId,
        UserDTO? requestingUser,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(
                new(HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));
        }

        var elementId = Guid.NewGuid().ToString();
        var senderId = requestingUser.Id.ToString();
        var type = firestoreMessage.Type.ToLower();

        // Convert any DateTime entries to Firestore Timestamps
        var normalizedData = firestoreMessage.Data.ToDictionary(
            kv => kv.Key,
            kv =>
            {
                if (kv.Value is DateTime dt)
                    return Timestamp.FromDateTime(dt.ToUniversalTime());
                return kv.Value;
            });

        var element = new FirestoreConversationItemDTO
        {
            Id = elementId,
            ConversationId = conversationId.ToString(),
            SenderId = senderId,
            Type = type,
            Data = normalizedData
        };

        await firestoreRepository.AddAsync("conversationElements", element, cancellationToken);

        // Fetch conversation for metadata update
        var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
            "conversations", conversationId.ToString(), cancellationToken);

        if (conversation == null)
        {
            return ServiceResponse.CreateErrorResponse(
                new(HttpStatusCode.NotFound, "Conversation not found", ErrorCodes.EntityNotFound));
        }

        var receiverId = conversation.ParticipantIds.FirstOrDefault(id => id != senderId);

        // Update metadata if it's a regular message
        if (type == "message")
        {
            var content = normalizedData.TryGetValue("Content", out var c) ? c?.ToString() : "[message]";
            conversation.LastMessage = content;
            conversation.LastMessageAt = element.CreatedAt;

            if (!string.IsNullOrEmpty(receiverId))
            {
                conversation.UnreadCounts ??= new Dictionary<string, int>();
                conversation.UnreadCounts.TryAdd(receiverId, 0);
                conversation.UnreadCounts[receiverId]++;
            }

            await firestoreRepository.UpdateAsync("conversations", conversation, cancellationToken);
        }

        // Send notification
        var payload = new
        {
            Type = type,
            Timestamp = DateTime.UtcNow,
            ConversationId = conversationId,
            SenderId = senderId,
            Message = normalizedData.TryGetValue("Content", out var message) ? message?.ToString() : "[new message]"
        };

        switch (type)
        {
            case "request":
                await notifier.NotifyNewRequest(Guid.Parse(receiverId), payload);
                break;
            case "reply":
                await notifier.NotifyNewReply(Guid.Parse(receiverId), payload);
                break;
            default:
                await notifier.NotifyNewMessage(Guid.Parse(receiverId), payload);
                break;
        }

        return ServiceResponse.CreateSuccessResponse();
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
    public async Task<ServiceResponse<ConversationDTO>> GetConversationByUsers(
        Guid currentUserId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);
        if (user is { Role: UserRoleEnum.Admin })
            return ServiceResponse.CreateErrorResponse<ConversationDTO>(CommonErrors.NotAllowed);

        var conversationKey = user.Role == UserRoleEnum.Client
            ? $"{currentUserId}_{userId}"
            : $"{userId}_{currentUserId}";

        var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
            "conversations",
            q => q.WhereEqualTo("Participants", conversationKey),
            cancellationToken);

        if (conversation == null)
        {
            return ServiceResponse.CreateErrorResponse<ConversationDTO>(
                new ErrorMessage(HttpStatusCode.NotFound, "Conversation not found", ErrorCodes.EntityNotFound));
        }

        var conversationId = Guid.Parse(conversation.Id);

        // ✅ Get all items in order
        var elements = await firestoreRepository.ListAsync<FirestoreConversationItemDTO>(
            "conversationElements",
            q => q.WhereEqualTo("ConversationId", conversationId.ToString()).OrderBy("CreatedAt"),
            cancellationToken);

        // ✅ Process unread messages
        var unreadMessages = elements
            .Where(e =>
                e.Type == "message" &&
                e.Data.TryGetValue("IsRead", out var isRead) &&
                isRead is bool and false &&
                e.SenderId != currentUserId.ToString())
            .ToList();

        foreach (var message in unreadMessages)
        {
            messageUpdateQueue.Enqueue(message.Id);
        }

        if (conversation.UnreadCounts != null &&
            conversation.UnreadCounts.TryGetValue(currentUserId.ToString(), out var unreadCount) &&
            unreadCount > 0)
        {
            conversation.UnreadCounts[currentUserId.ToString()] = 0;
            await firestoreRepository.UpdateAsync("conversations", conversation, cancellationToken);
        }

        // ✅ Extract other user info
        var isClient = conversation.ClientData.UserId == currentUserId.ToString();
        var other = isClient ? conversation.SpecialistData : conversation.ClientData;

        var result = new ConversationDTO
        {
            ConversationId = conversationId,
            UserId = Guid.Parse(other.UserId),
            UserFullName = other.UserFullName,
            UserProfilePictureUrl = other.UserProfilePictureUrl,
            ConversationItems = elements
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