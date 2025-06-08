using System.Diagnostics;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class ConversationService(IRepository<WebAppDatabaseContext> repository, IMessageService messageService): IConversationService
{
    public async Task<ServiceResponse<UserConversationDTO>> GetExchange(Guid currentUserId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);

        if (user.Role == UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse<UserConversationDTO>(CommonErrors.NotAllowed);
        }

        if (user.Role == UserRoleEnum.Client)
        {
            // var requests = await repository.ListAsync(new RequestUserProjectionSpec(currentUserId, userId), cancellationToken);
            var swPostgres = Stopwatch.StartNew();
            var requests = await repository.ListAsync(new RequestUserProjectionSpec(currentUserId, userId), cancellationToken);
            if (requests.Count == 0)
                return ServiceResponse.CreateErrorResponse<UserConversationDTO>(CommonErrors.EntityNotFound);
            
            swPostgres.Stop();
            Console.WriteLine($"PostgreSQL time: {swPostgres.ElapsedMilliseconds} ms");
            
            var messages = await messageService.GetMessagesBetweenUsers(currentUserId, userId, cancellationToken);
            var unreadMessages = messages
                .Where(m => m.ReceiverId == currentUserId.ToString() && !m.IsRead)
                .ToList();

            foreach (var unread in unreadMessages)
            {
                await messageService.MarkMessageAsRead(unread.Id!, cancellationToken);
                unread.IsRead = true;
            }
            
            var receiverUser = await repository.GetAsync(new UserSpec(userId), cancellationToken);
            if (receiverUser == null)
                return ServiceResponse.CreateErrorResponse<UserConversationDTO>(CommonErrors.EntityNotFound);
            var userExchangeDTO = new UserConversationDTO
            {
                Id = receiverUser.Id,
                FullName = receiverUser.FullName,
                Requests = requests,
                Messages = messages
            };
            
            return ServiceResponse.CreateSuccessResponse(userExchangeDTO);
        }
        else
        {
            var swPostgres = Stopwatch.StartNew();
            var requests = await repository.ListAsync(new RequestSpecialistProjectionSpec(userId, currentUserId), cancellationToken);
            
            if (requests.Count == 0)
                return ServiceResponse.CreateErrorResponse<UserConversationDTO>(CommonErrors.EntityNotFound);
            
            var messages = await messageService.GetMessagesBetweenUsers(userId, currentUserId, cancellationToken);
            
            var unreadMessages = messages
                .Where(m => m.ReceiverId == currentUserId.ToString() && !m.IsRead)
                .ToList();
            
            swPostgres.Stop();
            Console.WriteLine($"PostgreSQL time: {swPostgres.ElapsedMilliseconds} ms");

            foreach (var unread in unreadMessages)
            {
                await messageService.MarkMessageAsRead(unread.Id!, cancellationToken);
                unread.IsRead = true;
            }
            
            var senderUser = await repository.GetAsync(new UserSpec(userId), cancellationToken);
            if (senderUser == null)
                return ServiceResponse.CreateErrorResponse<UserConversationDTO>(CommonErrors.EntityNotFound);
            var userExchangeDTO = new UserConversationDTO
            {
                Id = senderUser.Id,
                FullName = senderUser.FullName,
                Requests = requests,
                Messages = messages
            };
            
            return ServiceResponse.CreateSuccessResponse(userExchangeDTO);
        }
    }
    
    public async Task<ServiceResponse<List<ConversationDTO>>> GetExchanges(Guid currentUserId, CancellationToken cancellationToken)
    {
        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);
        
        if (user.Role == UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse<List<ConversationDTO>>(CommonErrors.NotAllowed);
        }
        
        if (user.Role == UserRoleEnum.Client)
        {
            var requests = await repository.ListAsync(
                new RequestUserProjectionSpec(currentUserId, orderByCreatedAt: true),
                cancellationToken);

            var userIds = requests.Select(r => r.ReceiverUserId).Distinct();

            var users = await repository.ListAsync(new UserSpec(userIds), cancellationToken);

            var result = users.Select(u => new ConversationDTO
            {
                Id = u.Id,
                FullName = u.FullName,
            }).ToList();

            return ServiceResponse.CreateSuccessResponse(result);
        }
        else
        {
            var requests = await repository.ListAsync(
                new RequestSpecialistProjectionSpec(currentUserId, orderByCreatedAt: true),
                cancellationToken);

            var userIds = requests.Select(r => r.SenderUserId).Distinct();

            var users = await repository.ListAsync(new UserSpec(userIds), cancellationToken);

            var result = users.Select(u => new ConversationDTO
            {
                Id = u.Id,
                FullName = u.FullName,
            }).ToList();

            return ServiceResponse.CreateSuccessResponse(result);
        }
    }
}