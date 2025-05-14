using ExpertEase.Application.DataTransferObjects.TransactionDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Entities;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("api/profile/user/transactions")]
[Tags("UserTransactions")]
public class TransactionController(IUserService userService, ITransactionService transactionService) : AuthorizedController(userService)
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] TransactionAddDTO transaction)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.AddTransaction(transaction, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<TransactionDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.GetTransaction(new TransactionUserProjectionSpec(id, currentUser.Result.Id), id)) :
            CreateErrorMessageResult<TransactionDTO>(currentUser.Error);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<TransactionDTO>>>> GetPage(
        [FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.GetTransactions(new TransactionUserProjectionSpec(pagination.Search, currentUser.Result.Id), pagination)) :
            CreateErrorMessageResult<PagedResponse<TransactionDTO>>(currentUser.Error);
    }
    
    [Authorize]
    [HttpPatch("{id:guid}/cancel")]
    public async Task<ActionResult<RequestResponse>> CancelTransaction([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.CancelTransaction(id, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}