using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("/api/profile/requests/{requestId}/replies")]
public class ReplyController(IUserService userService, IReplyService replyService) : AuthorizedController(userService)
{
    
    [Authorize(Roles = "Client")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ReplyDTO>>> GetById(Guid requestId, [FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.GetReply(new ReplyUserProjectionSpec(id, requestId, currentUser.Result.Id), id)) :
            CreateErrorMessageResult<ReplyDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Client")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<ReplyDTO>>>> GetPage(
        Guid requestId, [FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.GetReplies(new ReplyUserProjectionSpec(pagination.Search, requestId,  currentUser.Result.Id), pagination)) :
            CreateErrorMessageResult<PagedResponse<ReplyDTO>>(currentUser.Error);
    }
    
    [Authorize(Roles = "Client")]
    [HttpPatch("{id:guid}/accept")]
    public async Task<ActionResult<RequestResponse>> Accept([FromBody] ReplyUpdateDTO reply)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.UpdateReply(reply, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Client")]
    [HttpPatch("{id:guid}/reject")]
    public async Task<ActionResult<RequestResponse>> Reject([FromBody] ReplyUpdateDTO reply)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.UpdateReply(reply, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}