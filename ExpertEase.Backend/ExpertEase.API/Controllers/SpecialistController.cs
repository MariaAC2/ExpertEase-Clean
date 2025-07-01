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
        
    // Replace the existing GetPage method with this:
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> GetPage(
        [FromQuery] PaginationSearchQueryParams pagination,
        [FromQuery] SpecialistFilterParams? filter = null)
    {
        // Validate filter parameters if provided
        if (filter != null)
        {
            // Validate rating range if provided
            if (filter.MinRating is < 0 or > 5 ||
                filter.MaxRating is < 0 or > 5 ||
                (filter is { MinRating: not null, MaxRating: not null } && filter.MinRating > filter.MaxRating))
            {
                return CreateErrorMessageResult<PagedResponse<SpecialistDTO>>(
                    new ErrorMessage(HttpStatusCode.BadRequest, 
                    "Invalid rating range. Ratings must be between 0 and 5, and minRating must be less than or equal to maxRating."));
            }

            // Validate experience range if provided
            if (!string.IsNullOrWhiteSpace(filter.ExperienceRange))
            {
                var validRanges = new[] { "0-2", "2-5", "5-7", "7-10", "10+" };
                if (!validRanges.Contains(filter.ExperienceRange.ToLowerInvariant()))
                {
                    return CreateErrorMessageResult<PagedResponse<SpecialistDTO>>(
                        new ErrorMessage(HttpStatusCode.BadRequest,
                        "Invalid experience range. Valid ranges are: 0-2, 2-5, 5-7, 7-10, 10+"));
                }
            }

            // Validate sort parameter if provided
            if (!string.IsNullOrWhiteSpace(filter.SortByRating))
            {
                var validSorts = new[] { "asc", "desc" };
                if (!validSorts.Contains(filter.SortByRating.ToLowerInvariant()))
                {
                    return CreateErrorMessageResult<PagedResponse<SpecialistDTO>>(
                        new ErrorMessage(HttpStatusCode.BadRequest,
                        "Invalid sort parameter. Valid values are: asc, desc"));
                }
            }
        }

        var specialistPagination = new SpecialistPaginationQueryParams
        {
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            Search = pagination.Search,
            Filter = filter ?? new SpecialistFilterParams()
        };

        return CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialists(specialistPagination));
    }

    // Update the legacy methods to use the nested Filter structure:

    // Replace SearchByCategory method:
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> SearchByCategory([FromQuery] Guid categoryId, [FromQuery] PaginationQueryParams pagination)
    {
        var specialistPagination = new SpecialistPaginationQueryParams
        {
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            Filter = new SpecialistFilterParams { CategoryId = categoryId }
        };

        return CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialists(specialistPagination));
    }

    // Replace SearchByRatingRange method:
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> SearchByRatingRange([FromQuery] int minRating, [FromQuery] int maxRating, [FromQuery] PaginationQueryParams pagination)
    {
        if (minRating < 0 || minRating > 5 || maxRating < 0 || maxRating > 5 || minRating > maxRating)
        {
            return CreateErrorMessageResult<PagedResponse<SpecialistDTO>>(new ErrorMessage(HttpStatusCode.BadRequest,
                "Invalid rating range."));
        }

        var specialistPagination = new SpecialistPaginationQueryParams
        {
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            Filter = new SpecialistFilterParams 
            { 
                MinRating = minRating,
                MaxRating = maxRating
            }
        };
        
        return CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialists(specialistPagination));
    }

    // Replace SearchByExperienceRange method:
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> SearchByExperienceRange([FromQuery] string experienceRange, [FromQuery] PaginationQueryParams pagination)
    {
        var validRanges = new[] { "0-2", "2-5", "5-7", "7-10", "10+" };
        
        if (string.IsNullOrWhiteSpace(experienceRange) || !validRanges.Contains(experienceRange.ToLowerInvariant()))
        {
            return CreateErrorMessageResult<PagedResponse<SpecialistDTO>>(new ErrorMessage(HttpStatusCode.BadRequest,
                "Invalid experience range. Valid ranges are: 0-2, 2-5, 5-7, 7-10, 10+"));
        }

        var specialistPagination = new SpecialistPaginationQueryParams
        {
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            Filter = new SpecialistFilterParams 
            { 
                ExperienceRange = experienceRange
            }
        };
        
        return CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialists(specialistPagination));
    }

    // Replace GetTopRated method:
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> GetTopRated([FromQuery] PaginationQueryParams pagination)
    {
        var specialistPagination = new SpecialistPaginationQueryParams
        {
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            Filter = new SpecialistFilterParams 
            { 
                SortByRating = "desc"
            }
        };

        return CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialists(specialistPagination));
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