namespace ExpertEase.Application.Services;

public interface IStripeAccountService
{
    Task<string> CreateConnectedAccount(string email);
    Task<string> GenerateOnboardingLink(string accountId);
    Task<string> CreatePaymentIntent(decimal amount, string stripeAccountId);
}