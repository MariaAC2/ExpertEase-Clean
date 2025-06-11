using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;

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