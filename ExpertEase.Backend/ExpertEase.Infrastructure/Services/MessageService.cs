using System.Net;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class MessageService(IFirebaseRepository firebaseRepository) : IMessageService
{
    public async Task<ServiceResponse> AddMessage(MessageAddDTO message, UserDTO? requestingUser, CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));
        }
        
        var messageEntity = new Message
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = requestingUser.Id.ToString(),
            ReceiverId = message.ReceiverId.ToString(),
            Content = message.Content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        
        await firebaseRepository.AddAsync("messages", messageEntity, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<List<MessageDTO>> GetMessagesBetweenUsers(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default)
    {
        var user1 = user1Id.ToString();
        var user2 = user2Id.ToString();

        var messages = await firebaseRepository.ListAsync<Message>(
            "messages",
            col => col
                .WhereIn("SenderId", new[] { user1, user2 })
                .WhereIn("ReceiverId", new[] { user1, user2 })
                .OrderBy("CreatedAt"),
            cancellationToken);

        var dtos = messages.Select(m => new MessageDTO
        {
            Id = m.Id,
            SenderId = m.SenderId,
            ReceiverId = m.ReceiverId,
            Content = m.Content,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        }).ToList();

        return dtos;
    }
    public async Task<ServiceResponse> MarkMessageAsRead(string messageId, CancellationToken cancellationToken = default)
    {
        var message = await firebaseRepository.GetAsync<Message>("messages", messageId, cancellationToken);

        if (message == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Message not found"));

        message.IsRead = true;

        await firebaseRepository.UpdateAsync("messages", message, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }
}