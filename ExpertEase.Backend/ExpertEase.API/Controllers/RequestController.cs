using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class RequestController(IUserService userService, IRequestService requestService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Client")]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] RequestAddDTO request)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await requestService.AddRequest(request, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    // [HttpGet("{id:guid}")]
    // public async Task<ActionResult<RequestResponse<RequestDTO>>> GetById([FromRoute] Guid id)
    // {
    //     var currentUser = await GetCurrentUser();
    //
    //     return currentUser.Result != null ? 
    //         CreateRequestResponseFromServiceResponse(await requestService.GetRequest(new RequestProjectionSpec(id))) : 
    //         CreateErrorMessageResult<RequestDTO>(currentUser.Error);
    // }
    
    // [Authorize(Roles = "Client")]
    // [HttpGet]
    // public async Task<ActionResult<RequestResponse<PagedResponse<RequestDTO>>>> GetPage(
    //     [FromQuery] PaginationSearchQueryParams pagination, [FromQuery] Guid userId)
    // {
    //     var currentUser = await GetCurrentUser();
    //
    //     return currentUser.Result != null ?
    //         CreateRequestResponseFromServiceResponse(await requestService.GetRequests(new RequestUserProjectionSpec(pagination.Search, currentUser.Result.Id, userId), pagination)) :
    //         CreateErrorMessageResult<PagedResponse<RequestDTO>>(currentUser.Error);
    // }

    [Authorize(Roles = "Client")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] RequestUpdateDTO request)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await requestService.UpdateRequest(request, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Client")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Cancel([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
        {
            return CreateErrorMessageResult(currentUser.Error);
        }
        
        var reply = new StatusUpdateDTO
        {
            Id = id,
            Status = Domain.Enums.StatusEnum.Cancelled,
        };

        return CreateRequestResponseFromServiceResponse(
            await requestService.UpdateRequestStatus(reply, currentUser.Result));
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}")]
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
    [HttpPatch("{id:guid}")]
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