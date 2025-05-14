using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.UserDTOs;

namespace ExpertEase.Application.DataTransferObjects.LoginDTOs;

/// <summary>
/// This DTO is used to respond to a login with the JWT token and user information.
/// </summary>
public class LoginResponseDTO
{
    [Required]
    public string Token { get; set; } = null!;
    [Required]
    public UserDTO User { get; set; } = null!;
}
