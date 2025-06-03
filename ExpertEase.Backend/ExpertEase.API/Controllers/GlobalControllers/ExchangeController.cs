using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class ExchangeController(IUserService userService, IExchangeService exchangeService): AuthorizedController(userService)
{
    [Authorize]
    [HttpGet("{senderId:guid}")]
    public async Task<ActionResult<RequestResponse<UserExchangeDTO>>> GetById([FromRoute] Guid senderId)
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await exchangeService.GetExchange(currentUser.Result.Id, senderId)) : 
            CreateErrorMessageResult<UserExchangeDTO>(currentUser.Error);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<UserExchangeDTO>>>> GetPaged([FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();
        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await exchangeService.GetExchanges(currentUser.Result.Id, pagination)) : 
            CreateErrorMessageResult<PagedResponse<UserExchangeDTO>>(currentUser.Error);
    }
}