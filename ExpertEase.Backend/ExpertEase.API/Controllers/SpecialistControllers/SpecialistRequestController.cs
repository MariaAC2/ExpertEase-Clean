using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.SpecialistControllers;

[ApiController]
[Route("/api/specialist/requests")]
[Tags("SpecialistRequests")]
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
        [FromQuery] PaginationSearchQueryParams pagination, [FromQuery] Guid userId)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await requestService.GetRequests(new RequestSpecialistProjectionSpec(pagination.Search, userId, currentUser.Result.Id), pagination)) :
            CreateErrorMessageResult<PagedResponse<RequestDTO>>(currentUser.Error);
    }

    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}/accept")]
    public async Task<ActionResult<RequestResponse>> Accept([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
        {
            return CreateErrorMessageResult(currentUser.Error);
        }
        
        var request = new StatusUpdateDTO
        {
            Id = id,
            Status = Domain.Enums.StatusEnum.Accepted
        };

        return CreateRequestResponseFromServiceResponse(
            await requestService.UpdateRequestStatus(request, currentUser.Result));
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}/reject")]
    public async Task<ActionResult<RequestResponse>> Reject([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
        {
            return CreateErrorMessageResult(currentUser.Error);
        }
        
        var request = new StatusUpdateDTO
        {
            Id = id,
            Status = Domain.Enums.StatusEnum.Rejected
        };

        return CreateRequestResponseFromServiceResponse(
            await requestService.UpdateRequestStatus(request, currentUser.Result));
    }
}