using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("/api/messages/")]
public class ExchangeController(IUserService userService, IExchangeService exchangeService): AuthorizedController(userService)
{
    [HttpGet("{senderId:guid}")]
    [Authorize]
    public async Task<ActionResult<RequestResponse<UserExchangeDTO>>> GetExchange([FromRoute] Guid senderId)
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await exchangeService.GetExchange(currentUser.Result.Id, senderId)) : 
            CreateErrorMessageResult<UserExchangeDTO>(currentUser.Error);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<RequestResponse<PagedResponse<UserExchangeDTO>>>> GetExchanges([FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await exchangeService.GetExchanges(currentUser.Result.Id, pagination)) : 
            CreateErrorMessageResult<PagedResponse<UserExchangeDTO>>(currentUser.Error);
    }
}