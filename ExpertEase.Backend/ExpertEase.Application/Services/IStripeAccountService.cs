using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.DataTransferObjects.StripeAccountDTOs;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface IStripeAccountService
{
    /// <summary>
    /// Creates a new Stripe connected account for a specialist
    /// </summary>
    Task<string> CreateConnectedAccount(string email);

    /// <summary>
    /// Generates onboarding link for account setup
    /// </summary>
    Task<ServiceResponse<StripeAccountLinkResponseDTO>> GenerateOnboardingLink(string accountId);

    /// <summary>
    /// Generates dashboard link for account management
    /// </summary>
    Task<ServiceResponse<StripeAccountLinkResponseDTO>> GenerateDashboardLink(string accountId);

    /// <summary>
    /// ✅ DEPRECATED: Legacy payment intent creation
    /// </summary>
    [Obsolete("Use CreatePaymentIntent(CreatePaymentIntentDTO) instead")]
    Task<string> CreatePaymentIntent(decimal amount, string stripeAccountId);

    /// <summary>
    /// ✅ NEW: Enhanced payment intent creation with escrow support
    /// </summary>
    Task<PaymentIntentResponseDTO> CreatePaymentIntent(CreatePaymentIntentDTO dto);

    /// <summary>
    /// ✅ NEW: Transfer money to specialist when service is completed
    /// </summary>
    Task<ServiceResponse<string>> TransferToSpecialist(
        string paymentIntentId, 
        string specialistAccountId, 
        decimal amount, 
        string reason = "Service completed");

    /// <summary>
    /// ✅ NEW: Refund money to client if service fails
    /// </summary>
    Task<ServiceResponse<string>> RefundPayment(
        string paymentIntentId, 
        decimal refundAmount, 
        string reason = "Service cancelled");

    /// <summary>
    /// Gets account status and capabilities
    /// </summary>
    Task<ServiceResponse<StripeAccountStatusDTO>> GetAccountStatus(string accountId);
}