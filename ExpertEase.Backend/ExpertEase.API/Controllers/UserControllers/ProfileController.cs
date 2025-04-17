using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("api/profile/user")]
public class ProfileController(IUserService userService, ISpecialistService specialistService) : AuthorizedController(userService)
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<UserDTO>>> Get()
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await UserService.GetUser(currentUser.Result.Id)) : 
            CreateErrorMessageResult<UserDTO>(currentUser.Error);
    }
    
    [Authorize]
    [HttpPatch("update")]
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
    
    [Authorize(Roles = "Client")]
    [HttpPut("become_specialist")]
    public async Task<ActionResult<RequestResponse>> BecomeSpecialist([FromBody] SpecialistAddDTO specialist)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.AddSpecialist(specialist, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}