using ExpertEase.Application.DataTransferObjects.TransactionDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
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
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Cancel([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.CancelTransaction(id, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    // [Authorize(Roles = "Admin")]
    // [HttpGet("{id:guid}")]
    // public async Task<ActionResult<RequestResponse<TransactionDTO>>> GetById([FromRoute] Guid id)
    // {
    //     var currentUser = await GetCurrentUser();
    //
    //     return currentUser.Result != null ?
    //         CreateRequestResponseFromServiceResponse(await transactionService.GetTransaction(new TransactionProjectionSpec(id), id)) :
    //         CreateErrorMessageResult<TransactionDTO>(currentUser.Error);
    // }
    //
    // [Authorize(Roles = "Admin")]
    // [HttpGet]
    // public async Task<ActionResult<RequestResponse<PagedResponse<TransactionDTO>>>> GetPage(
    //     [FromQuery] PaginationSearchQueryParams pagination)
    // {
    //     var currentUser = await GetCurrentUser();
    //
    //     return currentUser.Result != null ?
    //         CreateRequestResponseFromServiceResponse(await transactionService.GetTransactions(new TransactionProjectionSpec(pagination.Search), pagination)) :
    //         CreateErrorMessageResult<PagedResponse<TransactionDTO>>(currentUser.Error);
    // }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] TransactionUpdateDTO transaction)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.UpdateTransaction(transaction, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Delete([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.DeleteTransaction(id, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}