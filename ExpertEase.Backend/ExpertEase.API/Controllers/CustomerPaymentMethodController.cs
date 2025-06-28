using ExpertEase.Application.DataTransferObjects.CustomerPaymentMethodDTOs;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class CustomerPaymentMethodController(IUserService userService, ICustomerPaymentMethodService customerPaymentMethod) : AuthorizedController(userService)
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse<CustomerPaymentMethodDto>>> Add([FromBody] SaveCustomerPaymentMethodDto paymentMethod)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await customerPaymentMethod.SaveCustomerPaymentMethod(paymentMethod, currentUser.Result))
            : CreateErrorMessageResult<CustomerPaymentMethodDto>(currentUser.Error);
    }
}