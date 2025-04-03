using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/admin/users/[action]")]
public class UserController(IUserService _userService) : AuthorizedController(_userService)
{
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<UserDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await UserService.GetUser(id)) : 
            CreateErrorMessageResult<UserDTO>(currentUser.Error);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] UserAddDTO user)
    {
        var currentUser = await GetCurrentUser();
        user.Password = PasswordUtils.HashPassword(user.Password);

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.AddUser(user, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize]
    [HttpPut("{id:guid}")]
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
    
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Delete([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.DeleteUser(id)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}
