using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.GlobalControllers;

[ApiController]
[Route("/api/[controller]/[action]")]
[Tags("UserReplies")]
public class ReplyController(IUserService userService, IReplyService replyService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Specialist")]
    [HttpPost("{requestId:guid}")]
    public async Task<ActionResult<RequestResponse>> Add([FromRoute] Guid requestId, [FromBody] ReplyAddDTO reply)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null
            ? CreateRequestResponseFromServiceResponse(await replyService.AddReply(requestId, reply, currentUser.Result))
            : CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Client")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Accept([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
        {
            return CreateErrorMessageResult(currentUser.Error);
        }
        
        var reply = new StatusUpdateDTO
        {
            Id = id,
            Status = Domain.Enums.StatusEnum.Accepted
        };

        return CreateRequestResponseFromServiceResponse(await replyService.UpdateReplyStatus(reply, currentUser.Result));
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
    
    [Authorize(Roles = "Client")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Reject([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
        {
            return CreateErrorMessageResult(currentUser.Error);
        }
        
        var reply = new StatusUpdateDTO
        {
            Id = id,
            Status = Domain.Enums.StatusEnum.Rejected
        };

        return CreateRequestResponseFromServiceResponse(
            await replyService.UpdateReplyStatus(reply, currentUser.Result));
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}/cancel")]
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
            Status = Domain.Enums.StatusEnum.Cancelled
        };

        return CreateRequestResponseFromServiceResponse(
            await replyService.UpdateReplyStatus(reply, currentUser.Result));
    }
}