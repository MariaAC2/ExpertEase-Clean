using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("api/profile/specialist/categories")]
public class SpecialistCategoryController(IUserService userService, ICategoryService categoryService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Specialist")]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] CategorySpecialistDTO category)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.AddCategoryToSpecialist(category, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<CategoryDTO>>>> GetPage([FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();
    
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.GetCategoriesForSpecialist(currentUser.Result.Id, pagination)) :
            CreateErrorMessageResult<PagedResponse<CategoryDTO>>(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<CategoryDTO>>> GetByName([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
    
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.GetCategoryForSpecialist(id, currentUser.Result.Id)) :
            CreateErrorMessageResult<CategoryDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Delete([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();
    
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.DeleteCategoryFromSpecialist(id, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}