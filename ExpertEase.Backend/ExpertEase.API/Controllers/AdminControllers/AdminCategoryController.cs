using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using ExpertEase.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.AdminControllers;

[ApiController]
[Route("api/admin/categories")]
[Tags("AdminCategories")]
public class AdminCategoryController(IUserService userService, ICategoryService categoryService) : AuthorizedController(userService)
{
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Add([FromBody] CategoryAddDTO category)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.AddCategory(category, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<CategoryAdminDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.GetCategory(id)) :
            CreateErrorMessageResult<CategoryAdminDTO>(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<CategoryAdminDTO>>>> GetPage(
        [FromQuery] PaginationSearchQueryParams pagination)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.GetCategoriesAdmin(pagination)) :
            CreateErrorMessageResult<PagedResponse<CategoryAdminDTO>>(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] CategoryUpdateDTO category)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.UpdateCategory(category, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Delete([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.DeleteCategory(id, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}