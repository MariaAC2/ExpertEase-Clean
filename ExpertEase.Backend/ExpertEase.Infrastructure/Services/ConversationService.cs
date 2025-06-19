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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ExpertEase.Infrastructure.Services;

public class ConversationService(
    IRepository<WebAppDatabaseContext> repository,
    IFirestoreRepository firestoreRepository,
    IMessageUpdateQueue messageUpdateQueue,
    IConversationNotifier notifier,
    IMemoryCache cache,
    ILogger<ConversationService> logger): IConversationService
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
     
public async Task<ServiceResponse> AddConversationItem(
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

    // Fetch conversation for validation and metadata update
    var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
        "conversations", conversationId.ToString(), cancellationToken);

    if (conversation == null)
    {
        return ServiceResponse.CreateErrorResponse(
            new ErrorMessage(HttpStatusCode.NotFound, "Conversation not found", ErrorCodes.EntityNotFound));
    }

    // 🆕 Validate request status for messages and photos
    if (type == "message" || type == "photo")
    {
        var requestValidation = await ValidateRequestStatusForConversation(conversation.RequestId, cancellationToken);
        if (!requestValidation.IsOk)
        {
            return requestValidation;
        }
    }

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

    var receiverId = conversation.ParticipantIds.FirstOrDefault(id => id != senderId);

    // Update metadata if it's a regular message or photo
    if (type is "message" or "photo")
    {
        string content;
        if (type == "message")
        {
            content = normalizedData.TryGetValue("Content", out var c) ? c?.ToString() : "[message]";
        }
        else // type == "photo"
        {
            var caption = normalizedData.TryGetValue("caption", out var cap) ? cap?.ToString() : "";
            content = string.IsNullOrEmpty(caption) ? "📷 Photo" : $"📷 {caption}";
        }

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
        Message = type switch
        {
            "message" => normalizedData.TryGetValue("Content", out var message) ? message?.ToString() : "[new message]",
            "photo" => normalizedData.TryGetValue("caption", out var caption) && !string.IsNullOrEmpty(caption?.ToString()) 
                ? $"📷 {caption}" : "📷 Photo",
            _ => "[new message]"
        }
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

// 🆕 Add this new validation method to ConversationService
    private async Task<ServiceResponse> ValidateRequestStatusForConversation(
        string requestId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(requestId) || !Guid.TryParse(requestId, out var requestGuid))
        {
            return ServiceResponse.CreateErrorResponse(
                new ErrorMessage(HttpStatusCode.BadRequest, "Invalid request ID in conversation", ErrorCodes.CannotAdd));
        }

        // Get the request from your database (assuming you have a Request entity and specification)
        var request = await repository.GetAsync(new RequestSpec(requestGuid), cancellationToken);
        
        if (request == null)
        {
            return ServiceResponse.CreateErrorResponse(
                new ErrorMessage(HttpStatusCode.NotFound, "Request not found", ErrorCodes.EntityNotFound));
        }

        // Check if request status allows messaging
        // Adjust these status checks based on your RequestStatusEnum values
        if (request.Status == StatusEnum.Pending)
        {
            return ServiceResponse.CreateErrorResponse(
                new ErrorMessage(HttpStatusCode.Forbidden, 
                    "Cannot send messages or photos while the request is pending approval", 
                    ErrorCodes.CannotAdd));
        }

        if (request.Status == StatusEnum.Rejected)
        {
            return ServiceResponse.CreateErrorResponse(
                new ErrorMessage(HttpStatusCode.Forbidden, 
                    "Cannot send messages or photos for a rejected request", 
                    ErrorCodes.CannotAdd));
        }

        // Add any other status checks as needed
        // For example, if you have a "Completed" status that should also block messaging:
        // if (request.Status == RequestStatusEnum.Completed)
        // {
        //     return ServiceResponse.CreateErrorResponse(
        //         new ErrorMessage(HttpStatusCode.Forbidden, 
        //             "Cannot send messages or photos for a completed request", 
        //             ErrorCodes.CannotAdd));
        // }

        return ServiceResponse.CreateSuccessResponse();
    }
    
    // In your backend service that fetches from Firestore and returns to API
    public async Task<ConversationItemDTO> GetConversationItem(string id, CancellationToken cancellationToken = default)
    {
        var data = await firestoreRepository.GetAsync<FirestoreConversationItemDTO>(
            collection: "conversationElements",
            id,
            cancellationToken);
        
        if (data == null)
        {
            throw new KeyNotFoundException($"Conversation item with ID {id} not found.");
        }
        
        if (data.Data.TryGetValue("RequestedStartDate", out object? value) && value is Timestamp ts)
        {
            data.Data["RequestedStartDate"] = ts.ToDateTime();
        }
        
        if (data.Data.TryGetValue("StartDate", out value) && value is Timestamp startTs)
        {
            data.Data["StartDate"] = startTs.ToDateTime();
        }
        
        if (data.Data.TryGetValue("EndDate", out value) && value is Timestamp endTs)
        {
            data.Data["EndDate"] = endTs.ToDateTime();
        }
    
        return new ConversationItemDTO
        {
            Id = Guid.Parse(id),
            ConversationId = data.ConversationId,
            SenderId = data.SenderId,
            Type = data.Type,
            CreatedAt = data.CreatedAt.ToDateTime(),
            Data = data.Data.ToDictionary(kv => kv.Key, kv => kv.Value)
        };
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
    public async Task<ServiceResponse<PagedResponse<ConversationItemDTO>>> GetConversationByUsers(
        Guid currentUserId,
        Guid userId,
        PaginationQueryParams pagination,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Validate pagination parameters
        if (pagination.Page < 1) pagination.Page = 1;
        if (pagination.PageSize < 1 || pagination.PageSize > 100) 
            pagination.PageSize = 50;

        var cacheKey = $"conversation_items_{currentUserId}_{userId}_p{pagination.Page}_s{pagination.PageSize}";

        // 1. Try cache first
        if (cache.TryGetValue(cacheKey, out PagedResponse<ConversationItemDTO> cachedResult))
        {
            logger.LogInformation("Cache hit for conversation items {CacheKey}", cacheKey);
            return ServiceResponse.CreateSuccessResponse(cachedResult);
        }

        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);
        if (user is { Role: UserRoleEnum.Admin })
            return ServiceResponse.CreateErrorResponse<PagedResponse<ConversationItemDTO>>(CommonErrors.NotAllowed);

        var conversationKey = user.Role == UserRoleEnum.Client
            ? $"{currentUserId}_{userId}"
            : $"{userId}_{currentUserId}";

        // 2. Get conversation metadata
        var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
            "conversations",
            q => q.WhereEqualTo("Participants", conversationKey),
            cancellationToken);

        if (conversation == null)
        {
            return ServiceResponse.CreateErrorResponse<PagedResponse<ConversationItemDTO>>(
                new ErrorMessage(HttpStatusCode.NotFound, "Conversation not found", ErrorCodes.EntityNotFound));
        }

        var conversationId = Guid.Parse(conversation.Id);

        // 3. Get total count for pagination (cached separately for performance)
        var totalCountCacheKey = $"conversation_total_{conversationId}";
        if (!cache.TryGetValue(totalCountCacheKey, out int totalCount))
        {
            var countQuery = await firestoreRepository.ListAsync<FirestoreConversationItemDTO>(
                "conversationElements",
                q => q.WhereEqualTo("ConversationId", conversationId.ToString())
                      .Limit(1000), // Limit for performance, adjust based on your needs
                cancellationToken);
            
            totalCount = countQuery.Count;
            cache.Set(totalCountCacheKey, totalCount, TimeSpan.FromMinutes(5));
        }

        // 4. Calculate pagination
        var skip = (pagination.Page - 1) * pagination.PageSize;

        // 5. Get paginated conversation elements
        var elements = await firestoreRepository.ListAsync<FirestoreConversationItemDTO>(
            "conversationElements",
            q => q.WhereEqualTo("ConversationId", conversationId.ToString())
                  .OrderByDescending("CreatedAt") // Most recent first
                  .Offset(skip)
                  .Limit(pagination.PageSize),
            cancellationToken);

        // 🆕 6. Convert FirestoreConversationItemDTO to ConversationItemDTO with timestamp conversion
        var processedElements = elements.Select(element => 
        {
            // Convert timestamp fields in Data dictionary
            var processedData = ConvertTimestampsInData(element.Data);

            return new ConversationItemDTO
            {
                Id = Guid.Parse(element.Id),
                ConversationId = element.ConversationId,
                SenderId = element.SenderId,
                Type = element.Type,
                CreatedAt = element.CreatedAt.ToDateTime(),
                Data = processedData
            };
        }).ToList();

        // 7. Process unread messages (background task)
        _ = ProcessUnreadMessagesAsync(elements, currentUserId, conversation, cancellationToken);

        // 8. Create PagedResponse with ConversationItemDTO
        var pagedResult = new PagedResponse<ConversationItemDTO>(
            page: pagination.Page,
            pageSize: pagination.PageSize,
            totalCount: totalCount,
            data: processedElements.OrderBy(e => e.CreatedAt).ToList() // Chronological order for display
        );

        // 9. Cache result
        var cacheExpiry = pagination.Page == 1 
            ? TimeSpan.FromMinutes(1)  // Recent messages cache shorter
            : TimeSpan.FromMinutes(5); // Older messages cache longer
            
        cache.Set(cacheKey, pagedResult, cacheExpiry);

        stopwatch.Stop();
        logger.LogInformation("Conversation items loaded in {ElapsedMs}ms (page {Page}/{TotalPages}, {ItemCount} items)", 
            stopwatch.ElapsedMilliseconds, 
            pagination.Page, 
            Math.Ceiling((double)totalCount / pagination.PageSize),
            processedElements.Count);

        return ServiceResponse.CreateSuccessResponse(pagedResult);
    }

    // 🆕 Helper method to convert timestamps in data dictionary
    private Dictionary<string, object> ConvertTimestampsInData(Dictionary<string, object> data)
    {
        var processedData = new Dictionary<string, object>();
        
        foreach (var kvp in data)
        {
            if (kvp.Value is Timestamp timestamp)
            {
                processedData[kvp.Key] = timestamp.ToDateTime();
            }
            else
            {
                processedData[kvp.Key] = kvp.Value;
            }
        }
        
        return processedData;
    }

    public async Task<ServiceResponse<PagedResponse<UserConversationDTO>>> GetConversationsByUsers(
        Guid currentUserId, 
        PaginationQueryParams pagination,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Validate pagination
        if (pagination.Page < 1) pagination.Page = 1;
        if (pagination.PageSize < 1 || pagination.PageSize > 50) 
            pagination.PageSize = 20;

        var cacheKey = $"user_conversations_{currentUserId}_p{pagination.Page}_s{pagination.PageSize}";
        
        if (cache.TryGetValue(cacheKey, out PagedResponse<UserConversationDTO>? cachedConversations))
        {
            logger.LogInformation("Cache hit for user conversations {CacheKey}", cacheKey);
            return ServiceResponse.CreateSuccessResponse(cachedConversations);
        }

        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);
        
        if (user.Role == UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse<PagedResponse<UserConversationDTO>>(CommonErrors.NotAllowed);
        }

        // Get total count of conversations for this user
        var totalCountCacheKey = $"user_conversations_total_{currentUserId}";
        if (!cache.TryGetValue(totalCountCacheKey, out int totalCount))
        {
            var allConversations = await firestoreRepository.ListAsync<FirestoreConversationDTO>(
                "conversations",
                collection => collection
                    .WhereArrayContains("ParticipantIds", currentUserId.ToString())
                    .Limit(500), // Reasonable limit for counting
                cancellationToken);
            
            totalCount = allConversations.Count;
            cache.Set(totalCountCacheKey, totalCount, TimeSpan.FromMinutes(5));
        }

        var skip = (pagination.Page - 1) * pagination.PageSize;

        // Get paginated conversations
        var conversations = await firestoreRepository.ListAsync<FirestoreConversationDTO>(
            "conversations",
            collection => collection
                .WhereArrayContains("ParticipantIds", currentUserId.ToString())
                .OrderByDescending("LastMessageAt")
                .Offset(skip)
                .Limit(pagination.PageSize),
            cancellationToken);

        var conversationDTOs = conversations.Select(c =>
        {
            var isClient = c.ClientData.UserId == currentUserId.ToString();
            var other = isClient ? c.SpecialistData : c.ClientData;
        
            return new UserConversationDTO
            {
                UserId = other.UserId,
                UserFullName = other.UserFullName,
                UserProfilePictureUrl = other.UserProfilePictureUrl,
                LastMessage = c.LastMessage,
                LastMessageAt = c.LastMessageAt.ToDateTime(),
                UnreadCount = c.UnreadCounts?.GetValueOrDefault(currentUserId.ToString(), 0) ?? 0
            };
        }).ToList();

        var pagedResult = new PagedResponse<UserConversationDTO>(
            page: pagination.Page,
            pageSize: pagination.PageSize,
            totalCount: totalCount,
            data: conversationDTOs
        );

        // Cache result
        cache.Set(cacheKey, pagedResult, TimeSpan.FromMinutes(2));
        
        stopwatch.Stop();
        logger.LogInformation("User conversations loaded in {ElapsedMs}ms (page {Page}/{TotalPages}, {Count} conversations)", 
            stopwatch.ElapsedMilliseconds, 
            pagination.Page,
            Math.Ceiling((double)totalCount / pagination.PageSize),
            conversationDTOs.Count);
            
        return ServiceResponse.CreateSuccessResponse(pagedResult);
    }

    // Background processing method
    private async Task ProcessUnreadMessagesAsync(
        List<FirestoreConversationItemDTO> elements,
        Guid currentUserId,
        FirestoreConversationDTO conversation,
        CancellationToken cancellationToken)
    {
        try
        {
            // Process unread messages
            var unreadMessages = elements
                .Where(e => e.Type == "message" &&
                           e.Data.TryGetValue("IsRead", out var isRead) &&
                           isRead is bool and false &&
                           e.SenderId != currentUserId.ToString())
                .ToList();

            foreach (var message in unreadMessages)
            {
                messageUpdateQueue.Enqueue(message.Id);
            }

            // Update conversation unread count
            if (conversation.UnreadCounts?.TryGetValue(currentUserId.ToString(), out var unreadCount) == true && 
                unreadCount > 0)
            {
                conversation.UnreadCounts[currentUserId.ToString()] = 0;
                await firestoreRepository.UpdateAsync("conversations", conversation, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing unread messages for user {UserId}", currentUserId);
        }
    }

    // Keep legacy method for backward compatibility
    public async Task<ServiceResponse<ConversationDTO>> GetConversationByUsersLegacy(
        Guid currentUserId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Use the new paginated method with default pagination
        var paginationParams = new PaginationQueryParams { Page = 1, PageSize = 50 };
        var pagedResult = await GetConversationByUsers(currentUserId, userId, paginationParams, cancellationToken);
        
        if (!pagedResult.IsOk)
        {
            return ServiceResponse.CreateErrorResponse<ConversationDTO>(pagedResult.Error);
        }

        // Convert PagedResponse back to ConversationDTO for legacy compatibility
        var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
            "conversations",
            q => q.WhereEqualTo("Participants", $"{currentUserId}_{userId}"),
            cancellationToken);

        if (conversation == null) 
        {
            return ServiceResponse.CreateErrorResponse<ConversationDTO>(
                new ErrorMessage(HttpStatusCode.NotFound, "Conversation not found", ErrorCodes.EntityNotFound));
        }

        var isClient = conversation.ClientData.UserId == currentUserId.ToString();
        var other = isClient ? conversation.SpecialistData : conversation.ClientData;

        var legacyResult = new ConversationDTO
        {
            ConversationId = Guid.Parse(conversation.Id),
            UserId = Guid.Parse(other.UserId),
            UserFullName = other.UserFullName,
            UserProfilePictureUrl = other.UserProfilePictureUrl,
            ConversationItems = pagedResult.Result.Data.Select(item => new FirestoreConversationItemDTO
            {
                Id = item.Id.ToString(),
                ConversationId = item.ConversationId,
                SenderId = item.SenderId,
                Type = item.Type,
                Data = item.Data.ToDictionary(kv => kv.Key, kv => kv.Value),
                CreatedAt = Timestamp.FromDateTime(item.CreatedAt.ToUniversalTime())
            }).ToList()
        };

        return ServiceResponse.CreateSuccessResponse(legacyResult);
    }

    public async Task<ServiceResponse> AddConvElement(
        FirestoreConversationItemAddDTO firestoreMessage,
        Guid conversationId,
        UserDTO? requestingUser,
        CancellationToken cancellationToken = default)
    {
        // Your existing implementation...
        var result = await AddConversationItem(firestoreMessage, conversationId, requestingUser, cancellationToken);

        if (!result.IsOk) return null;
        // Invalidate all relevant caches including total counts
        var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
            "conversations", conversationId.ToString(), cancellationToken);
                
        if (conversation != null)
        {
            InvalidateConversationCaches(conversationId, conversation.ParticipantIds);
        }

        return result;
    }

    private void InvalidateConversationCaches(Guid conversationId, List<string> participantIds)
    {
        foreach (var participantId in participantIds)
        {
            // Remove total count caches
            cache.Remove($"conversation_total_{conversationId}");
            cache.Remove($"user_conversations_total_{participantId}");
            
            // Remove paginated caches
            for (int page = 1; page <= 20; page++)
            {
                for (int pageSize = 10; pageSize <= 100; pageSize += 10)
                {
                    cache.Remove($"user_conversations_{participantId}_p{page}_s{pageSize}");
                    
                    var otherParticipants = participantIds.Where(id => id != participantId);
                    foreach (var otherId in otherParticipants)
                    {
                        cache.Remove($"conversation_items_{participantId}_{otherId}_p{page}_s{pageSize}");
                    }
                }
            }
        }
    }
}