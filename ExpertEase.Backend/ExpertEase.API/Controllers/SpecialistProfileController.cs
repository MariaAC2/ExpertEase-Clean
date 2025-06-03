using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SpecialistProfileController(IUserService userService, ISpecialistProfileService specialistService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Client")]
    [HttpPut]
    public async Task<ActionResult<RequestResponse<BecomeSpecialistResponseDTO>>> BecomeSpecialist([FromBody] BecomeSpecialistDTO becomeSpecialistProfile)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.AddSpecialistProfile(becomeSpecialistProfile, currentUser.Result)) :
            CreateErrorMessageResult<BecomeSpecialistResponseDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<SpecialistProfileDTO>>> Get()
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialistProfile(currentUser.Result.Id)) :
            CreateErrorMessageResult<SpecialistProfileDTO>(currentUser.Error);
    }
    
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