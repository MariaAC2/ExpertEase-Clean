using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;
using ExpertEase.Infrastructure.Firestore.FirestoreDTOs;

namespace ExpertEase.Application.Services;

public interface IConversationService
{
    Task AddConversation(Conversation conversation, CancellationToken cancellationToken = default);
    Task UpdateConversationRequestId(Guid conversationId, Guid requestId,
        CancellationToken cancellationToken = default);
    Task<ServiceResponse<ConversationDTO>> GetConversationByUsers(Guid currentUserId, Guid senderUserId, CancellationToken cancellationToken = default);
    Task<ServiceResponse<List<UserConversationDTO>>> GetConversationsByUsers(Guid currentUserId, CancellationToken cancellationToken);

    Task<ServiceResponse> AddConvElement(FirestoreConversationItemAddDTO firestoreMessage, Guid conversationId,
        UserDTO? requestingUser, CancellationToken cancellationToken = default);

}