using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SpecialistController(IUserService userService, ISpecialistService specialistService) : AuthorizedController(userService)
{
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<SpecialistDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialist(id)) : 
            CreateErrorMessageResult<SpecialistDTO>(currentUser.Error);
    }
    
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> GetPage([FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();
    
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialists(pagination)) :
            CreateErrorMessageResult<PagedResponse<SpecialistDTO>>(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] SpecialistAddDTO user)
    {
        var currentUser = await GetCurrentUser();
        user.Password = PasswordUtils.HashPassword(user.Password);
    
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.AddSpecialist(user, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] SpecialistUpdateDTO specialist)
    {
        var currentUser = await GetCurrentUser();

        if (currentUser.Result == null)
        {
            return CreateErrorMessageResult(currentUser.Error);
        }

        var response = await specialistService.UpdateSpecialist(specialist, currentUser.Result);

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
