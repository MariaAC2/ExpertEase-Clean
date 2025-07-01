using System.Net;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SpecialistController(IUserService userService, ISpecialistService specialistService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<SpecialistDTO>>> GetById([FromRoute] Guid id)
    {
        var result = await GetCurrentUser();
        
        return result.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialist(id, result.Result)) :
            CreateErrorMessageResult<SpecialistDTO>(result.Error);
    }
    
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> GetPage([FromQuery] SpecialistPaginationQueryParams pagination)
    {
        return CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialists(pagination));
    }
    
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> SearchByCategory([FromQuery] Guid categoryId, [FromQuery] PaginationQueryParams pagination)
    {
        return CreateRequestResponseFromServiceResponse(await specialistService.SearchSpecialistsByCategory(categoryId, pagination));
    }
    
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> SearchByRatingRange([FromQuery] int minRating, [FromQuery] int maxRating, [FromQuery] PaginationQueryParams pagination)
    {
        if (minRating < 0 || minRating > 5 || maxRating < 0 || maxRating > 5 || minRating > maxRating)
        {
            // return BadRequest(new RequestResponse<PagedResponse<SpecialistDTO>>
            // {
            //     IsSuccess = false,
            //     Message = "Invalid rating range. Ratings must be between 0 and 5, and minRating must be less than or equal to maxRating."
            // });
            return CreateErrorMessageResult<PagedResponse<SpecialistDTO>>(new ErrorMessage(HttpStatusCode.BadRequest,
                "Invalid rating range."));
        }
        
        return CreateRequestResponseFromServiceResponse(await specialistService.SearchSpecialistsByRatingRange(minRating, maxRating, pagination));
    }
    
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> SearchByExperienceRange([FromQuery] string experienceRange, [FromQuery] PaginationQueryParams pagination)
    {
        var validRanges = new[] { "0-2", "2-5", "5-7", "7-10", "10+" };
        
        if (string.IsNullOrWhiteSpace(experienceRange) || !validRanges.Contains(experienceRange.ToLowerInvariant()))
        {
            // return BadRequest(new RequestResponse<PagedResponse<SpecialistDTO>>
            // {
            //     ErrorMessage = "Invalid experience range. Valid ranges are: 0-2, 2-5, 5-7, 7-10, 10+"
            // });
            return CreateErrorMessageResult<PagedResponse<SpecialistDTO>>(new ErrorMessage(HttpStatusCode.BadRequest,
                "Invalid experience range. Valid ranges are: 0-2, 2-5, 5-7, 7-10, 10+"));
        }
        
        return CreateRequestResponseFromServiceResponse(await specialistService.SearchSpecialistsByExperienceRange(experienceRange, pagination));
    }
    
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> GetTopRated([FromQuery] PaginationQueryParams pagination)
    {
        return CreateRequestResponseFromServiceResponse(await specialistService.GetTopRatedSpecialists(pagination));
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] SpecialistAddDTO user)
    {
        var currentUser = await GetCurrentUser();
        user.Password = PasswordUtils.HashPassword(user.Password);
    
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.AddSpecialist(user, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] SpecialistUpdateDTO specialist)
    {
        var currentUser = await GetCurrentUser();

        if (currentUser.Result == null)
        {
            return CreateErrorMessageResult(currentUser.Error);
        }

        var response = await specialistService.UpdateSpecialist(specialist, currentUser.Result);

        return CreateRequestResponseFromServiceResponse(response);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Delete([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.DeleteUser(id)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}