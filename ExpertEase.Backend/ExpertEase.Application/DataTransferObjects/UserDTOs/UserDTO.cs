using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.ReviewDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using Google.Type;
using DateTime = System.DateTime;

namespace ExpertEase.Application.DataTransferObjects.UserDTOs;

/// <summary>
/// This DTO is used to transfer information about a user within the application and to client application.
/// Note that it doesn't contain a password property and that is why you should use DTO rather than entities to use only the data that you need or protect sensible information.
/// </summary>
public class UserDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string FullName { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public UserRoleEnum Role { get; set; }
    [Required]
    public string ProfilePictureUrl { get; set; } = null!;
    [Required]
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;
}

public class UserAdminDetailsDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string FullName { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public UserRoleEnum Role { get; set; }
    [Required]
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public DateTime UpdatedAt { get; set; }
    [Required]
    public int Rating { get; set; } = 0;
    public string? ProfilePictureUrl { get; set; }
    public ContactInfoDTO? ContactInfo { get; set; }
    public SpecialistProfileDTO? Specialist { get; set; }
}

public class ContactInfoDTO
{
    [Required]
    public string PhoneNumber { get; set; } = null!;
    [Required]
    public string Address { get; set; } = null!;
}

public class UserDetailsDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;

    public string? ProfilePictureUrl { get; set; }

    [Required]
    public int Rating { get; set; }

    [Required]
    public List<ReviewDTO> Reviews { get; set; } = [];

    // Specialist-only fields (null if client)
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public int? YearsExperience { get; set; }
    public string? Description { get; set; }
    public List<string>? Portfolio { get; set; }
    public List<string>? Categories { get; set; }
}

public class UserProfileDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Specialist-only fields (null if client)
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string? Address { get; set; }
    public int? YearsExperience { get; set; }
    public string? Description { get; set; }
    public string? StripeAccountId { get; set; }
    public List<string>? Portfolio { get; set; }
    public List<string>? Categories { get; set; }
}

public class UserPaymentDetailsDTO
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string UserFullName { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string PhoneNumber { get; set; }
}