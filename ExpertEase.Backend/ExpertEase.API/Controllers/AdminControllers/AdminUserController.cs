using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/users/")]
[Tags("AdminUsers")]
public class AdminUserController(IUserService userService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<UserDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await UserService.GetUserAdmin(id)) : 
            CreateErrorMessageResult<UserDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<UserDTO>>>> GetPage([FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.GetUsers(pagination)) :
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
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] UserUpdateDTO user)
    {
        var currentUser = await GetCurrentUser();

        if (currentUser.Result == null)
        {
            return CreateErrorMessageResult(currentUser.Error);
        }

        var updatedUserDto = user with
        {
            Password = !string.IsNullOrWhiteSpace(user.Password)
                ? PasswordUtils.HashPassword(user.Password)
                : null
        };

        var response = await UserService.UpdateUser(updatedUserDto, currentUser.Result);

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
}
