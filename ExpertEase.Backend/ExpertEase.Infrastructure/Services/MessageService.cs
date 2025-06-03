using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class MessageService(IFirebaseRepository firebaseRepository) : IMessageService
{
    public async Task<ServiceResponse<Message>> AddMessage(Message message, CancellationToken cancellationToken = default)
    {
        var addedMessage = await firebaseRepository.AddAsync("messages", message, cancellationToken);
        return ServiceResponse.CreateSuccessResponse(addedMessage);
    }

    public async Task<ServiceResponse<List<Message>>> GetMessagesBetweenUsers(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default)
    {
        // Assuming a method exists in the repository to get messages between two users
        var messages = await firebaseRepository.ListAsync<Message>("messages", cancellationToken);
        var filteredMessages = messages.Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                                                   (m.SenderId == user2Id && m.ReceiverId == user1Id)).ToList();
        return ServiceResponse.CreateSuccessResponse(filteredMessages);
    }
}