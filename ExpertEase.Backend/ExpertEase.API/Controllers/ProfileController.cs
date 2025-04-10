using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/profile/")]
public class ProfileController(IUserService userService, ISpecialistService specialistService): AuthorizedController(userService)
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<UserDTO>>> GetProfile()
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await UserService.GetUser(currentUser.Result.Id)) : 
            CreateErrorMessageResult<UserDTO>(currentUser.Error);
    }
    
    [Authorize]
    [HttpPatch("update")]
    public async Task<ActionResult<RequestResponse>> UpdateProfile([FromBody] UserUpdateDTO user)
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
    [HttpPatch("become_specialist")]
    public async Task<ActionResult<RequestResponse>> BecomeSpecialist([FromBody] SpecialistAddDTO specialist)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.AddSpecialist(specialist, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}