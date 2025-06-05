using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class ConversationController(IUserService userService, IConversationService conversationService): AuthorizedController(userService)
{
    [Authorize]
    [HttpGet("{senderId:guid}")]
    public async Task<ActionResult<RequestResponse<UserConversationDTO>>> GetById([FromRoute] Guid senderId)
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await conversationService.GetExchange(currentUser.Result.Id, senderId)) : 
            CreateErrorMessageResult<UserConversationDTO>(currentUser.Error);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<List<ConversationDTO>>>> GetPage()
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await conversationService.GetExchanges(currentUser.Result.Id)) : 
            CreateErrorMessageResult<List<ConversationDTO>>(currentUser.Error);
    }
}