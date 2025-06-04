using ExpertEase.Application.DataTransferObjects.MessageDTOs;
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
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class ExchangeService(IRepository<WebAppDatabaseContext> repository, IMessageService messageService): IExchangeService
{
    public async Task<ServiceResponse<UserExchangeDTO>> GetExchange(Guid currentUserId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);

        if (user.Role == UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse<UserExchangeDTO>(CommonErrors.NotAllowed);
        }

        if (user.Role == UserRoleEnum.Client)
        {
            var requests = await repository.ListAsync(new RequestUserProjectionSpec(currentUserId, userId), cancellationToken);
            
            if (requests.Count == 0)
                return ServiceResponse.CreateErrorResponse<UserExchangeDTO>(CommonErrors.EntityNotFound);
            
            var messages = await messageService.GetMessagesBetweenUsers(currentUserId, userId, cancellationToken);
            
            var receiverUser = await repository.GetAsync(new UserSpec(userId), cancellationToken);
            if (receiverUser == null)
                return ServiceResponse.CreateErrorResponse<UserExchangeDTO>(CommonErrors.EntityNotFound);
            var userExchangeDTO = new UserExchangeDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Requests = requests,
                Messages = messages
            };
            
            return ServiceResponse.CreateSuccessResponse(userExchangeDTO);
        }
        else
        {
            var requests = await repository.ListAsync(new RequestSpecialistProjectionSpec(userId, currentUserId), cancellationToken);
            
            if (requests.Count == 0)
                return ServiceResponse.CreateErrorResponse<UserExchangeDTO>(CommonErrors.EntityNotFound);
            var messages = await messageService.GetMessagesBetweenUsers(userId, currentUserId, cancellationToken);
            
            var senderUser = await repository.GetAsync(new UserSpec(userId), cancellationToken);
            if (senderUser == null)
                return ServiceResponse.CreateErrorResponse<UserExchangeDTO>(CommonErrors.EntityNotFound);
            var userExchangeDTO = new UserExchangeDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Requests = requests,
                Messages = messages
            };
            
            return ServiceResponse.CreateSuccessResponse(userExchangeDTO);
        }
    }

    public async Task<ServiceResponse<PagedResponse<UserExchangeDTO>>> GetExchanges(Guid currentUserId,
        PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);
        
        if (user.Role == UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse<PagedResponse<UserExchangeDTO>>(CommonErrors.NotAllowed);
        }

        if (user.Role == UserRoleEnum.Client)
        {
            var requests = await repository.ListAsync(
                new RequestUserProjectionSpec(currentUserId, orderByCreatedAt: true),
                cancellationToken);
            
            var grouped = requests
                .GroupBy(r => r.ReceiverUserId)
                .ToList();
            var exchangeList = new List<UserExchangeDTO>();

            foreach (var group in grouped)
            {
                var senderId = group.Key;
                var senderUser = await repository.GetAsync(new UserSpec(senderId), cancellationToken);
                var messagesResponse = await messageService.GetMessagesBetweenUsers(currentUserId, senderId, cancellationToken);
                
                var unreadMessages = messagesResponse
                    .Where(m => m.ReceiverId == currentUserId.ToString() && !m.IsRead)
                    .ToList();

                foreach (var unread in unreadMessages)
                {
                    unread.IsRead = true;
                    await messageService.MarkMessageAsRead(unread.Id, cancellationToken);
                }
    
                var exchange = new UserExchangeDTO
                {
                    Id = senderId,
                    FullName = senderUser?.FullName ?? "N/A",
                    Requests = group.ToList(),
                    Messages = messagesResponse,
                };

                exchangeList.Add(exchange);
            }
            
            var totalCount = grouped.Count;

            var paged = exchangeList
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var result = new PagedResponse<UserExchangeDTO>(
                page: pagination.Page,
                pageSize: pagination.PageSize,
                totalCount: totalCount,
                data: paged
            );
            
            return ServiceResponse.CreateSuccessResponse(result);
        }
        else
        {
            var requests = await repository.ListAsync(
                new RequestSpecialistProjectionSpec(currentUserId, orderByCreatedAt: true),
                cancellationToken);
            
            var grouped = requests
                .GroupBy(r => r.SenderUserId)
                .ToList();
            var exchangeList = new List<UserExchangeDTO>();

            foreach (var group in grouped)
            {
                var senderId = group.Key;
                var senderUser = await repository.GetAsync(new UserSpec(senderId), cancellationToken);
                var messagesResponse = await messageService.GetMessagesBetweenUsers(senderId, currentUserId, cancellationToken);
                
                var unreadMessages = messagesResponse
                    .Where(m => m.ReceiverId == currentUserId.ToString() && !m.IsRead)
                    .ToList();

                foreach (var unread in unreadMessages)
                {
                    unread.IsRead = true;
                    await messageService.MarkMessageAsRead(unread.Id, cancellationToken);
                }
    
                var exchange = new UserExchangeDTO
                {
                    Id = senderId,
                    FullName = senderUser?.FullName ?? "N/A",
                    Requests = group.ToList(),
                    Messages = messagesResponse,
                };

                exchangeList.Add(exchange);
            }
            
            var totalCount = grouped.Count;

            var paged = exchangeList
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var result = new PagedResponse<UserExchangeDTO>(
                page: pagination.Page,
                pageSize: pagination.PageSize,
                totalCount: totalCount,
                data: paged
            );
            
            return ServiceResponse.CreateSuccessResponse(result);
        }
    }
}