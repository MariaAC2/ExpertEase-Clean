using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface IConversationService
{
    Task AddConversation(Conversation conversation, CancellationToken cancellationToken = default);

    Task UpdateConversationRequestId(Guid conversationId, Guid requestId,
        CancellationToken cancellationToken = default);
    public Task<ServiceResponse<UserConversationDTO>> GetExchange(Guid currentUserId, Guid senderUserId, CancellationToken cancellationToken = default);
    public Task<ServiceResponse<List<ConversationDTO>>> GetExchanges(Guid currentUserId, CancellationToken cancellationToken = default);
}