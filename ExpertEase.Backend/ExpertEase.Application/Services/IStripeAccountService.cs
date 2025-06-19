using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface IStripeAccountService
{
    Task<string> CreateConnectedAccount(string email);
    Task<ServiceResponse<StripeAccountLinkResponseDTO>> GenerateOnboardingLink(string accountId);
    Task<string> CreatePaymentIntent(decimal amount, string stripeAccountId);
}