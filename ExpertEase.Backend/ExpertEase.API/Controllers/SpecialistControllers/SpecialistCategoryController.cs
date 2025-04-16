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
[Route("api/specialists/categories")]
public class SpecialistCategoryController(IUserService userService, ICategoryService categoryService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Specialist")]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] string name)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.AddCategoryToSpecialist(name, currentUser.Result)) :
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
    [HttpGet("{name}")]
    public async Task<ActionResult<RequestResponse<CategoryDTO>>> GetByName([FromRoute] string name)
    {
        var currentUser = await GetCurrentUser();
    
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.GetCategoryForSpecialist(name, currentUser.Result.Id)) :
            CreateErrorMessageResult<CategoryDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{name}")]
    public async Task<ActionResult<RequestResponse>> Delete([FromRoute] string name)
    {
        var currentUser = await GetCurrentUser();
    
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.DeleteCategoryFromSpecialist(name, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}