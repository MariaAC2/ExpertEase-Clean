using System.Net;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Infrastructure.Firebase.FirestoreRepository;
using ExpertEase.Infrastructure.Firebase.FirestoreMappers;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class MessageService(IFirestoreRepository firestoreRepository) : IMessageService
{
    public async Task<ServiceResponse> AddMessage(MessageAddDTO message, UserDTO? requestingUser, CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));
        }

        var domainMessage = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = requestingUser.Id,
            Content = message.Content,
            IsRead = false,
            ConversationId = message.ConversationId,
            CreatedAt = DateTime.UtcNow
        };

        var firestoreDto = MessageMapper.ToFirestoreDTO(domainMessage);
        await firestoreRepository.AddAsync("messages", firestoreDto, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<List<MessageDTO>> GetMessagesBetweenUsers(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversationIdStr = conversationId.ToString();

        var messagesDto = await firestoreRepository.ListAsync<FirestoreMessageDTO>(
            "messages",
            col => col
                .WhereEqualTo("ConversationId", conversationIdStr)
                .OrderBy("CreatedAt"),
            cancellationToken);

        var domainMessages = messagesDto.Select(MessageMapper.FromFirestoreDTO).ToList();

        var dtos = domainMessages.Select(m => new MessageDTO
        {
            Id = m.Id,
            SenderId = m.SenderId,
            ConversationId = m.ConversationId,
            Content = m.Content,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        }).ToList();

        return dtos;
    }

    public async Task<ServiceResponse> MarkMessageAsRead(string messageId, CancellationToken cancellationToken = default)
    {
        var messageDto = await firestoreRepository.GetAsync<FirestoreMessageDTO>("messages", messageId, cancellationToken);

        if (messageDto == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Message not found"));

        messageDto.IsRead = true;

        await firestoreRepository.UpdateAsync("messages", messageDto, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }
}
