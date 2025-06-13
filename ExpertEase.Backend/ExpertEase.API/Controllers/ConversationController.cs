using System.Diagnostics;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using ExpertEase.Infrastructure.Firestore.FirestoreDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class ConversationController(IUserService userService, IConversationService conversationService): AuthorizedController(userService)
{
    [Authorize]
    [HttpGet("{senderId:guid}")]
    public async Task<ActionResult<RequestResponse<ConversationDTO>>> GetById([FromRoute] Guid senderId)
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await conversationService.GetConversationByUsers(currentUser.Result.Id, senderId)) : 
            CreateErrorMessageResult<ConversationDTO>(currentUser.Error);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<List<UserConversationDTO>>>> GetPage()
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await conversationService.GetConversationsByUsers(currentUser.Result.Id, CancellationToken.None)) : 
            CreateErrorMessageResult<List<UserConversationDTO>>(currentUser.Error);
    }
}