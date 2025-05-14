using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.AdminControllers;

[ApiController]
[Route("/api/admin/requests/{requestId:guid}/replies/")]
[Tags("AdminReplies")]
public class AdminReplyController(IUserService userService, IReplyService replyService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ReplyDTO>>> GetById(Guid requestId, [FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.GetReply(new ReplyProjectionSpec(id, requestId))) :
            CreateErrorMessageResult<ReplyDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<ReplyDTO>>>> GetPage(Guid requestId,
        [FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.GetReplies(new ReplyProjectionSpec(pagination.Search, requestId), pagination)) :
            CreateErrorMessageResult<PagedResponse<ReplyDTO>>(currentUser.Error);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Delete([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await replyService.DeleteReply(id, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}