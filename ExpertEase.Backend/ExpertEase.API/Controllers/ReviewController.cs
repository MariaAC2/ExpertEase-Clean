﻿using ExpertEase.Application.DataTransferObjects.ReviewDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class ReviewController(IUserService userService, IReviewService reviewService):AuthorizedController(userService)
{
    [Authorize]
    [HttpPost("{serviceTaskId:guid}")]
    public async Task<ActionResult<RequestResponse>> Add([FromRoute] Guid serviceTaskId, [FromBody] ReviewAddDTO review)
    {
        var currentUser = await GetCurrentUser();
    
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await reviewService.AddReview(serviceTaskId, review, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
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
    public async Task<ActionResult<RequestResponse<PagedResponse<ReviewDTO>>>> GetPage([FromQuery] PaginationQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await reviewService.GetReviewsList(currentUser.Result.Id, pagination)) :
            CreateErrorMessageResult<PagedResponse<ReviewDTO>>(currentUser.Error);
    }
}