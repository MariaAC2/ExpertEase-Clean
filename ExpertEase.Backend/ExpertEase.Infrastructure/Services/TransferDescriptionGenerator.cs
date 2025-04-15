using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Infrastructure.Services;

public class TransferDescriptionGenerator : ITransferDescriptionGenerator
{
    public string Generate(Request request, Reply reply)
    {
        return $"User {request.SenderUser.FirstName} {request.SenderUser.LastName} has a problem with the following description {request.Description}." +
               $"Specialist {request.ReceiverUser.FirstName} {request.ReceiverUser.LastName} accepted solving the problem." +
               $"The service is at address {request.Address}, from {reply.StartDate:yyyy-MM-dd} to {reply.EndDate:yyyy-MM-dd} with a price of {reply.Price:C}." +
               $"User contact information: {request.SenderUser.Email}, {request.PhoneNumber}." +
               $"Specialist contact information: {request.ReceiverUser.Email}, {request.ReceiverUser.Specialist.PhoneNumber}.";
    }
}