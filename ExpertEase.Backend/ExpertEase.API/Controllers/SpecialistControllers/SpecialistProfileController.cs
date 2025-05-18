using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.SpecialistControllers;

[ApiController]
[Route("api/profile/specialist")]
[Tags("SpecialistProfile")]
public class SpecialistProfileController(IUserService userService, ISpecialistProfileService specialistService) : AuthorizedController(userService)
{
    // Gets specialist profile for specialist
    [Authorize(Roles = "Specialist")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<SpecialistProfileDTO>>> Get()
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialistProfile(currentUser.Result.Id)) :
            CreateErrorMessageResult<SpecialistProfileDTO>(currentUser.Error);
    }
    
    // Updates specialist profile for specialist
    [Authorize(Roles = "Specialist")]
    [HttpPatch]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] SpecialistProfileUpdateDTO specialistProfile)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.UpdateSpecialistProfile(specialistProfile, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}