using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.SpecialistControllers;

[ApiController]
[Route("/api/profile/specialist/requests/{requestId}/replies")]
[Tags("SpecialistReplies")]
public class SpecialistReplyController(IUserService userService, IReplyService replyService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Specialist")]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add(Guid requestId, [FromBody] ReplyAddDTO reply)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.AddReply(requestId, reply, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ReplyDTO>>> GetById(Guid requestId, [FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.GetReply(new ReplySpecialistProjectionSpec(id, requestId, currentUser.Result.Id))) :
            CreateErrorMessageResult<ReplyDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<ReplyDTO>>>> GetPage(
        Guid requestId, [FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.GetReplies(new ReplySpecialistProjectionSpec(pagination.Search, requestId,  currentUser.Result.Id), pagination)) :
            CreateErrorMessageResult<PagedResponse<ReplyDTO>>(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] ReplyUpdateDTO reply)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.UpdateReply(reply, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}