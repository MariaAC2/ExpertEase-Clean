using ExpertEase.Application.DataTransferObjects.ReviewDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.SpecialistControllers;

[ApiController]
[Route("/api/task/{taskId:guid}/confirm/reviews")]
public class SpecialistReviewController(IUserService userService, IReviewService reviewService): AuthorizedController(userService)
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] ReviewAddDTO review)
    {
        var currentUser = await GetCurrentUser();
    
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await reviewService.AddReview(review, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}