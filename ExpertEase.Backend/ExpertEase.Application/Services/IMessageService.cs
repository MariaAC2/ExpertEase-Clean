using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface IMessageService
{
    public Task<ServiceResponse> MarkMessageAsRead(string messageId, CancellationToken cancellationToken = default);
}