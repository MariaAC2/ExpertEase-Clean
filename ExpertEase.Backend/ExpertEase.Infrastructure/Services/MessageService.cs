using System.Net;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Infrastructure.Firebase.FirestoreRepository;
using ExpertEase.Infrastructure.Firebase.FirestoreMappers;
using ExpertEase.Infrastructure.Firestore.FirestoreDTOs;
using ExpertEase.Infrastructure.Repositories;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Services;

public class MessageService(IFirestoreRepository firestoreRepository) : IMessageService
{

    // public async Task<ServiceResponse> MarkMessageAsRead(string messageId, CancellationToken cancellationToken = default)
    // {
    //     var messageDto = await firestoreRepository.GetAsync<FirestoreMessageDTO>("messages", messageId, cancellationToken);
    //
    //     if (messageDto == null)
    //         return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Message not found"));
    //
    //     messageDto.IsRead = true;
    //
    //     await firestoreRepository.UpdateAsync("messages", messageDto, cancellationToken);
    //
    //     return ServiceResponse.CreateSuccessResponse();
    // }
    public Task<ServiceResponse> MarkMessageAsRead(string messageId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
