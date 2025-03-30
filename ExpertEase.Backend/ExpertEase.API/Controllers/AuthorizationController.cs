using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Enums;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;

/// <summary>
/// This is a controller to respond to authentication requests.
/// Inject the required services through the constructor.
/// </summary>
[ApiController]
[Route("api/[controller]/[action]")]
public class AuthorizationController(IUserService userService) : ResponseController
{
    [HttpPost]
    public async Task<ActionResult<RequestResponse<LoginResponseDTO>>> Login([FromBody] LoginDTO login) // The FromBody attribute indicates that the parameter is deserialized from the JSON body.
    {
        return CreateRequestResponseFromServiceResponse(await userService.Login(login with { Password = PasswordUtils.HashPassword(login.Password)})); // The "with" keyword works only with records and it creates another object instance with the updated properties. 
    }
    
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> Register([FromBody] UserRegisterDTO regDto)
    {
        var role = regDto.Email.EndsWith("@admin.com", StringComparison.OrdinalIgnoreCase)
            ? UserRoleEnum.Admin
            : UserRoleEnum.Client;
        
        var user = new UserAddDTO
        {
            FirstName = regDto.FirstName,
            LastName = regDto.LastName,
            Email = regDto.Email,
            Password = PasswordUtils.HashPassword(regDto.Password),
            Role = role
        };
        
        return CreateRequestResponseFromServiceResponse(await userService.AddUser(user));
    }
}
