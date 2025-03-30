using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects;

public class UserRegisterDTO
{
    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}