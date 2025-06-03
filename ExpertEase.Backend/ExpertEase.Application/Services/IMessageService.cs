using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface IMessageService
{
    public Task<ServiceResponse<Message>> AddMessage(Message message, CancellationToken cancellationToken = default);
    public Task<ServiceResponse<List<Message>>> GetMessagesBetweenUsers(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default);
}