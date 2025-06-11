using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class PaymentService(IRepository<WebAppDatabaseContext> repository): IPaymentService
{
    public async Task<ServiceResponse<Payment>> AddPayment(PaymentAddDTO paymentDTO, CancellationToken cancellationToken = default)
    {
        var service = await repository.GetAsync(new ServiceTaskSpec(paymentDTO.ServiceTaskId), cancellationToken);
        if (service == null)
        {
            return ServiceResponse.CreateErrorResponse<Payment>(new(
                System.Net.HttpStatusCode.NotFound, 
                "Service task with this id not found!", 
                ErrorCodes.EntityNotFound));
        }
        var payment = new Payment
        {
            ServiceTaskId = paymentDTO.ServiceTaskId,
            ServiceTask = service,
            Amount = paymentDTO.Amount,
            StripeAccountId = paymentDTO.StripeAccountId,
            Status = PaymentStatusEnum.Pending,
            Currency = "RON"
        };
        
        await repository.AddAsync(payment, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse(payment);
    }
}