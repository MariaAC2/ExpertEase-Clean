using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/stripe/account")]
public class StripeAccountController(IUserService userService, IStripeAccountService stripeService) : AuthorizedController(userService)
{
    [Authorize]
    [HttpPost("onboarding-link/{accountId}")]
    public async Task<ActionResult<RequestResponse<StripeAccountLinkResponseDTO>>> CreateLink([FromRoute] string accountId)
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await stripeService.GenerateOnboardingLink(accountId))
            : CreateErrorMessageResult<StripeAccountLinkResponseDTO>();
    }
}
