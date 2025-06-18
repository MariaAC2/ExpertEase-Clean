using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.UserDTOs;

/// <summary>
/// This DTO is used to update a user, the properties besides the id are nullable to indicate that they may not be updated if they are null.
/// </summary>
public record UserUpdateDTO
{
    [Required]
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Password { get; init; }
    public string? Address { get; init; }
    public string? PhoneNumber { get; init; }
}

public class UserUpdateResponseDTO
{
    public string Token { get; init; }
    public UserUpdateDTO User { get; init; }
}

public record AdminUserUpdateDTO
{
    [Required]
    public Guid Id { get; init; }
    public string? FullName { get; init; }
    public UserRoleEnum? Role { get; init; }
}