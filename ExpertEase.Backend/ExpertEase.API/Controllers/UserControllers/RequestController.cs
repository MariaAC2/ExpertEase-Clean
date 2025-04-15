using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.TransactionDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
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
[Route("/api/profile/requests")]
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

    [Authorize(Roles = "Client")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<RequestDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await requestService.GetRequest(new RequestUserProjectionSpec(id, currentUser.Result.Id))) : 
            CreateErrorMessageResult<RequestDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Client")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<RequestDTO>>>> GetPage(
        [FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await requestService.GetRequests(new RequestUserProjectionSpec(pagination.Search, currentUser.Result.Id), pagination)) :
            CreateErrorMessageResult<PagedResponse<RequestDTO>>(currentUser.Error);
    }

    [Authorize(Roles = "Client")]
    [HttpPatch("update")]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] RequestUpdateDTO request)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await requestService.UpdateRequest(request, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}