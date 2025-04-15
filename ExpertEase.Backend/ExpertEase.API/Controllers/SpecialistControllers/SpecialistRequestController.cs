using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("/api/profile/specialist/requests")]
public class SpecialistRequestController(IUserService userService, IRequestService requestService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Specialist")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<RequestDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await requestService.GetRequest(new RequestSpecialistProjectionSpec(id, currentUser.Result.Id))) : 
            CreateErrorMessageResult<RequestDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<RequestDTO>>>> GetPage(
        [FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await requestService.GetRequests(new RequestSpecialistProjectionSpec(pagination.Search, currentUser.Result.Id), pagination)) :
            CreateErrorMessageResult<PagedResponse<RequestDTO>>(currentUser.Error);
    }

    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}/accept")]
    public async Task<ActionResult<RequestResponse>> Accept([FromBody] RequestUpdateDTO request)
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await requestService.UpdateRequest(request, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}/reject")]
    public async Task<ActionResult<RequestResponse>> Reject([FromBody] RequestUpdateDTO request)
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await requestService.UpdateRequest(request, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}