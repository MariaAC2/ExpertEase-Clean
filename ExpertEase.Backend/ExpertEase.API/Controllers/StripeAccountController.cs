using ExpertEase.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/stripe/account")]
public class StripeAccountController(IStripeAccountService stripeService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateAccount()
    {
        var accountId = await stripeService.CreateConnectedAccount("test-specialist@example.com");
        return Ok(new { accountId });
    }

    [HttpPost("onboarding-link/{accountId}")]
    public async Task<IActionResult> CreateLink([FromRoute] string accountId)
    {
        var url = await stripeService.GenerateOnboardingLink(accountId);
        return Ok(new { url });
    }
    
    [HttpPost("create-payment-intent")]
    public async Task<IActionResult> CreatePaymentIntent()
    {
        var clientSecret = await stripeService.CreatePaymentIntent(350, "acct_1RY5dMIEuydUWNaj"); // Replace with test account
        return Ok(new { clientSecret });
    }
}
