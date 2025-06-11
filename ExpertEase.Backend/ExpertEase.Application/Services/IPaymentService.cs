using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface IPaymentService
{
    Task<ServiceResponse<Payment>> AddPayment(PaymentAddDTO paymentDTO, CancellationToken cancellationToken = default);
}