using System.ComponentModel.DataAnnotations;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.UserDTOs;

/// <summary>
/// This DTO is used to add a user, note that it doesn't have an id property because the id for the user entity should be added by the application.
/// </summary>
public class UserAddDTO
{
    [Required]
    public string FullName { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
    [Required]
    public UserRoleEnum Role { get; set; }
}

public class SocialUserInfo
{
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Picture { get; set; }
}

public class FacebookUserResponse
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public FacebookPicture? Picture { get; set; }
}

public class FacebookPicture
{
    public FacebookPictureData? Data { get; set; }
}

public class FacebookPictureData
{
    public string? Url { get; set; }
}