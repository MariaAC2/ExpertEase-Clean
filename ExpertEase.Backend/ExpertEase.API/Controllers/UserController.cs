using System.Net;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Enums;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UserController(IUserService userService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<UserDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.GetUserAdmin(id, currentUser.Result.Id)) :
            CreateErrorMessageResult<UserDTO>(currentUser.Error);
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<UserDTO>>> Get()
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.GetUser(currentUser.Result.Id)) :
            CreateErrorMessageResult<UserDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<UserDTO>>>> GetPage([FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.GetUsers(currentUser.Result.Id, pagination)) :
            CreateErrorMessageResult<PagedResponse<UserDTO>>(currentUser.Error);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] UserAddDTO user)
    {
        var currentUser = await GetCurrentUser();
        user.Password = PasswordUtils.HashPassword(user.Password);

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.AddUser(user, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] AdminUserUpdateDTO user)
    {
        var currentUser = await GetCurrentUser();

        var response = await UserService.AdminUpdateUser(user, currentUser.Result);

        return CreateRequestResponseFromServiceResponse(response);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Delete([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.DeleteUser(id)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize]
    [HttpPatch]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] UserUpdateDTO user)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.UpdateUser(user with
            {
                Password = !string.IsNullOrWhiteSpace(user.Password) ? PasswordUtils.HashPassword(user.Password) : null
            }, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}