using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/accounts/")]
[Tags("AdminAccounts")]
public class AdminAccountController(IUserService userService, IAccountService accountService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<AccountDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await accountService.GetAccount(id)) :
            CreateErrorMessageResult<AccountDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] AccountUpdateDTO account)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await accountService.UpdateAccount(account, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Delete([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await accountService.DeleteAccount(id, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}