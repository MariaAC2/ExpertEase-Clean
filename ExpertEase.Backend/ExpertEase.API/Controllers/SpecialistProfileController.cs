using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/profile/specialist")]
public class SpecialistProfileController(IUserService userService, ISpecialistService specialistService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Specialist")]
    [HttpPut("update")]
    public async Task<ActionResult<RequestResponse>> UpdateProfile([FromBody] SpecialistUpdateDTO specialist)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.UpdateSpecialist(specialist, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    // resolve request (accept or deny)
    // send reply
}