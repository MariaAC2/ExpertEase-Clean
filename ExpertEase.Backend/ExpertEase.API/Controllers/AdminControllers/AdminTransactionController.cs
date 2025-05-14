using ExpertEase.Application.DataTransferObjects.TransactionDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/transactions/")]
[Tags("AdminTransactions")]
public class AdminTransactionController(IUserService userService, ITransactionService transactionService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<TransactionDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.GetTransaction(new TransactionProjectionSpec(id), id)) :
            CreateErrorMessageResult<TransactionDTO>(currentUser.Error);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<TransactionDTO>>>> GetPage(
        [FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.GetTransactions(new TransactionProjectionSpec(pagination.Search), pagination)) :
            CreateErrorMessageResult<PagedResponse<TransactionDTO>>(currentUser.Error);
    }
    
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