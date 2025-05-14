using ExpertEase.Application.DataTransferObjects.TransactionDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.SpecialistControllers;

[ApiController]
[Route("/api/profile/specialist/transactions")]
[Tags("SpecialistTransactions")]
public class SpecialistTransactionController(IUserService userService, ITransactionService transactionService) : AuthorizedController(userService)
{
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<TransactionDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.GetTransaction(new TransactionSpecialistProjectionSpec(id, currentUser.Result.Id), id)) :
            CreateErrorMessageResult<TransactionDTO>(currentUser.Error);
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<TransactionDTO>>>> GetPage(
        [FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await transactionService.GetTransactions(new TransactionSpecialistProjectionSpec(pagination.Search, currentUser.Result.Id), pagination)) :
            CreateErrorMessageResult<PagedResponse<TransactionDTO>>(currentUser.Error);
    }
}