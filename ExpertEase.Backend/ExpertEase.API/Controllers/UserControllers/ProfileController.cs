using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("api/profile/user/")]
[Tags("UserProfile")]
public class ProfileController(IUserService userService, ISpecialistProfileService specialistService, ICategoryService categoryService) : AuthorizedController(userService)
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<RequestResponse<UserDTO>>> GetById()
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await UserService.GetUser(currentUser.Result.Id)) : 
            CreateErrorMessageResult<UserDTO>(currentUser.Error);
    }
    
    [Authorize]
    [HttpPatch]
    public async Task<ActionResult<RequestResponse>> Update([FromBody] UserUpdateDTO user)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await UserService.UpdateUser(user with
            {
                Password = !string.IsNullOrWhiteSpace(user.Password) ? PasswordUtils.HashPassword(user.Password) : null
            }, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Client")]
    [HttpPut("become_specialist")]
    public async Task<ActionResult<RequestResponse<BecomeSpecialistResponseDTO>>> BecomeSpecialist([FromBody] BecomeSpecialistDTO becomeSpecialistProfile)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.AddSpecialistProfile(becomeSpecialistProfile, currentUser.Result)) :
            CreateErrorMessageResult<BecomeSpecialistResponseDTO>(currentUser.Error);
    }
    
    [HttpGet("categories")]
    public async Task<ActionResult<RequestResponse<List<CategoryDTO>>>> GetCategories([FromQuery] string? search = null)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await categoryService.GetCategories(search)) :
            CreateErrorMessageResult<List<CategoryDTO>>(currentUser.Error);
    }
    
    // [HttpPost("categories")]
    // public async Task<ActionResult<RequestResponse>> AddCategory([FromBody] CategorySpecialistDTO category)
    // {
    //     var currentUser = await GetCurrentUser();
    //
    //     return currentUser.Result != null ?
    //         CreateRequestResponseFromServiceResponse(await categoryService.AddCategoryToSpecialist(category, currentUser.Result)) :
    //         CreateErrorMessageResult(currentUser.Error);
    // }
}