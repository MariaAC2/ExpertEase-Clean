using System.Net;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Stripe;

namespace ExpertEase.Infrastructure.Services;

public class StripeAccountService : IStripeAccountService
{
    private readonly StripeSettings _stripeConfiguration;

    public StripeAccountService(IOptions<StripeSettings> stripeConfiguration)
    {
        _stripeConfiguration = stripeConfiguration.Value;

        StripeConfiguration.ApiKey = _stripeConfiguration.SecretKey;
    }

    public async Task<string> CreateConnectedAccount(string email)
    {
        var service = new AccountService();

        var options = new AccountCreateOptions
        {
            Type = "express",
            Country = "RO",
            Email = email,
            Capabilities = new AccountCapabilitiesOptions
            {
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true }
            }
        };

        var account = await service.CreateAsync(options);
        return account.Id;
    }

    public async Task<ServiceResponse<StripeAccountLinkResponseDTO>> GenerateOnboardingLink(string accountId)
    {
        var linkService = new AccountLinkService();
        var link = await linkService.CreateAsync(new AccountLinkCreateOptions
        {
            Account = accountId,
            ReturnUrl = $"http://localhost:5241/return/{accountId}",
            RefreshUrl = $"http://localhost:5241/return/{accountId}",
            Type = "account_onboarding"
        });
        
        if (link == null || string.IsNullOrEmpty(link.Url))
        {
            return ServiceResponse.CreateErrorResponse<StripeAccountLinkResponseDTO>(
                new(HttpStatusCode.Forbidden ,"Failed to create account link. Please try again later."));
        }

        return ServiceResponse.CreateSuccessResponse(new StripeAccountLinkResponseDTO
        {
            Url = link.Url
        });
    }

    public async Task<string> CreatePaymentIntent(decimal amount, string stripeAccountId)
    {
        var service = new PaymentIntentService();

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // amount in cents
            Currency = "ron", // or "eur", etc.
            PaymentMethodTypes = new List<string> { "card" },

            TransferData = new PaymentIntentTransferDataOptions
            {
                Destination = stripeAccountId // 👈 this sends the money to the specialist
            },

            Description = "Plată pentru serviciu ExpertEase",
            Metadata = new Dictionary<string, string>
            {
                { "platform", "ExpertEase" }
            }
        };

        var paymentIntent = await service.CreateAsync(options);
        return paymentIntent.ClientSecret;
    }
}