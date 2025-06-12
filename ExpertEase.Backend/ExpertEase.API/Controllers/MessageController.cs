﻿using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class MessageController(IUserService userService, IMessageService messageService): AuthorizedController(userService)
{
    [Authorize]
    [HttpPost("{conversationId:guid}")]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] MessageAddDTO message, [FromRoute] Guid conversationId)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await messageService.AddMessage(message, conversationId, currentUser.Result)) : 
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize]
    [HttpGet("{user1Id:guid}/{user2Id:guid}")]
    // public async Task<ActionResult<RequestResponse<List<MessageDTO>>>> GetMessagesBetweenUsers([FromRoute] Guid user1Id, [FromRoute] Guid user2Id)
    // {
    //     var currentUser = await GetCurrentUser();
    //     
    //     return currentUser.Result != null ? 
    //         CreateRequestResponseFromServiceResponse(await messageService.GetMessagesBetweenUsers(user1Id, user2Id)) : 
    //         CreateErrorMessageResult<List<MessageDTO>>(currentUser.Error);
    // }
    
    [Authorize]
    [HttpPatch("{messageId:guid}")]
    public async Task<ActionResult<RequestResponse>> MarkMessageAsRead([FromRoute] Guid messageId)
    {
        var currentUser = await GetCurrentUser();
        
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await messageService.MarkMessageAsRead(messageId.ToString())) : 
            CreateErrorMessageResult(currentUser.Error);
    }
}