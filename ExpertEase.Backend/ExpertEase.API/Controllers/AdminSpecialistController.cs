using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/admin/specialists/[action]")]
public class SpecialistController(IUserService _userService, ISpecialistService _specialistService) : AuthorizedController(_userService)
{
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<SpecialistDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await _specialistService.GetSpecialist(id)) : 
            CreateErrorMessageResult<SpecialistDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet] // This attribute will make the controller respond to a HTTP GET request on the route /api/User/GetPage.
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> GetPage([FromQuery] PaginationSearchQueryParams pagination) // The FromQuery attribute will bind the parameters matching the names of
    // the PaginationSearchQueryParams properties to the object in the method parameter.
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await _specialistService.GetSpecialists(pagination)) :
            CreateErrorMessageResult<PagedResponse<SpecialistDTO>>(currentUser.Error);
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
    public async Task<ActionResult<RequestResponse>> Update([FromBody] SpecialistUpdateDTO specialist)
    {
        var currentUser = await GetCurrentUser();

        if (currentUser.Result == null)
        {
            return CreateErrorMessageResult(currentUser.Error);
        }

        var response = await _specialistService.UpdateSpecialist(specialist, currentUser.Result);

        return CreateRequestResponseFromServiceResponse(response);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Delete([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await _specialistService.DeleteSpecialist(id)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}
