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

    // ✅ UPDATED: Returns just success/failure
    Task<ServiceResponse> ConfirmPayment(
        PaymentConfirmationDTO confirmationDTO,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<PagedResponse<PaymentHistoryDTO>>> GetPaymentHistory(
        Guid userId,
        PaginationSearchQueryParams pagination,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse<PaymentDetailsDTO>> GetPaymentDetails(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    // ✅ UPDATED: Returns just success/failure
    Task<ServiceResponse> RefundPayment(
        PaymentRefundDTO refundDTO,
        CancellationToken cancellationToken = default);

    // ✅ UPDATED: Returns just success/failure
    Task<ServiceResponse> CancelPayment(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse> HandleStripeWebhook(string eventJson,
        string signature, CancellationToken cancellationToken = default);
}