﻿using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AccountController(IUserService userService, IAccountService accountService): AuthorizedController(userService)
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<AccountDTO>>> GetById()
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await accountService.GetUserAccount(currentUser.Result.Id)) : 
            CreateErrorMessageResult<AccountDTO>(currentUser.Error);
    }
    
    [Authorize]
    [HttpPatch]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] AccountUpdateDTO account)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await accountService.UpdateAccount(account, currentUser.Result)) : 
            CreateErrorMessageResult(currentUser.Error);
    }
}