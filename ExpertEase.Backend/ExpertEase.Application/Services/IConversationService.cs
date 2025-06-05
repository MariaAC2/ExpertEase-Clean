using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface IConversationService
{
    public Task<ServiceResponse<UserConversationDTO>> GetExchange(Guid currentUserId, Guid senderUserId, CancellationToken cancellationToken = default);
    public Task<ServiceResponse<List<ConversationDTO>>> GetExchanges(Guid currentUserId, CancellationToken cancellationToken = default);
}