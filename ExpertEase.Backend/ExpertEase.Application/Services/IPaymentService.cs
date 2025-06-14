using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface IPaymentService
{
    Task<ServiceResponse<Payment>> AddPayment(PaymentAddDTO paymentDTO, CancellationToken cancellationToken = default);

    Task<ServiceResponse<PaymentIntentResponseDTO>> CreatePaymentIntent(
        PaymentIntentCreateDTO createDTO,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<ServiceTask>> ConfirmPayment(
        PaymentConfirmationDTO confirmationDTO,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<PagedResponse<PaymentHistoryDTO>>> GetPaymentHistory(
        Guid userId,
        PaginationSearchQueryParams pagination,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<PaymentDetailsDTO>> GetPaymentDetails(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<Payment>> RefundPayment(
        PaymentRefundDTO refundDTO,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<Payment>> CancelPayment(
        Guid paymentId,
        CancellationToken cancellationToken = default);
}