using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.ReviewDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("/api/profile/reviews")]
public class ReviewController(IUserService userService, IReviewService reviewService):AuthorizedController(userService)
{
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ReviewDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await reviewService.GetReview(id, currentUser.Result.Id)) : 
            CreateErrorMessageResult<ReviewDTO>(currentUser.Error);
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<ReviewDTO>>>> GetPage(
        [FromQuery] PaginationSearchQueryParams pagination, [FromQuery] Guid userId)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await reviewService.GetReviews(currentUser.Result.Id, pagination)) :
            CreateErrorMessageResult<PagedResponse<ReviewDTO>>(currentUser.Error);
    }
}