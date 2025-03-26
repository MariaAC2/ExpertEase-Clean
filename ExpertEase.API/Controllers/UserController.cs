using Example.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Services;
using ExpertEase.Authorization;
using ExpertEase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;

namespace ExpertEase.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UsersController(IUserService _userService, ILogger<UsersController> _logger) : AuthorizedController(_userService)
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUser(id, cancellationToken);

        if (user == null)
            return NotFound($"User with ID {id} not found.");

        return Ok(user);
    }

    // [HttpGet]
    // public async Task<IActionResult> GetUsers([FromQuery] PaginationSearchQueryParams pagination, CancellationToken cancellationToken)
    // {
    //     var users = await _userService.GetUsers(pagination, cancellationToken);
    //
    //     return Ok(users);
    // }

    [HttpGet]
    public async Task<IActionResult> GetUserCount(CancellationToken cancellationToken)
    {
        var count = await _userService.GetUserCount(cancellationToken);

        return Ok(count);
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDTO login, CancellationToken cancellationToken)
    {
        var result = await _userService.Login(login, cancellationToken);
        
        if (result == null)
        {
            _logger.LogWarning("Login failed for email: {Email}", login.Email);
            return Unauthorized("Invalid email or password.");
        }

        return Ok(new
        {
            token = result.Token,
            user = result.User
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] UserAddDTO user, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUser();
        user.Password = PasswordUtils.HashPassword(user.Password);
        var success = await _userService.AddUser(user, currentUser, cancellationToken);
        
        if (!success)
            return Conflict("User already exists or you are not authorized to add users.");

        return Ok(success);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDTO userDto, CancellationToken cancellationToken)
    {
        if (id != userDto.Id)
            return BadRequest("Mismatched user ID.");

        var success = await _userService.UpdateUser(userDto, null, cancellationToken);

        if (!success)
            return NotFound($"User with ID {id} not found or you are not authorized.");

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var success = await _userService.DeleteUser(id, null, cancellationToken);

        if (!success)
            return NotFound($"User with ID {id} not found or you are not authorized.");

        return NoContent();
    }
}
