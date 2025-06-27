using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface IPaymentService
{
    // Existing methods
    Task<ServiceResponse<PaymentIntentResponseDTO>> CreatePaymentIntent(PaymentIntentCreateDTO createDTO, CancellationToken cancellationToken = default);
    Task<ServiceResponse> ConfirmPayment(PaymentConfirmationDTO confirmationDTO, CancellationToken cancellationToken = default);
    
    // ✅ NEW: Add these escrow methods
    Task<ServiceResponse> ReleasePayment(PaymentReleaseDTO releaseDTO, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PaymentStatusResponseDTO>> GetPaymentStatus(Guid paymentId, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PaymentReportDTO>> GetRevenueReport(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    
    // Existing methods
    Task<ServiceResponse<PagedResponse<PaymentHistoryDTO>>> GetPaymentHistory(Guid userId, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PaymentDetailsDTO>> GetPaymentDetails(Guid paymentId, CancellationToken cancellationToken = default);
    Task<ServiceResponse> RefundPayment(PaymentRefundDTO refundDTO, CancellationToken cancellationToken = default);
    Task<ServiceResponse> CancelPayment(Guid paymentId, CancellationToken cancellationToken = default);
    Task<ServiceResponse> HandleStripeWebhook(string eventJson, string signature, CancellationToken cancellationToken = default);
}