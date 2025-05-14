using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;

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
    
    public SpecialistUpdateDTO? Specialist { get; init; } = null;
}