namespace ExpertEase.Application.DataTransferObjects;

/// <summary>
/// This DTO is used to update a user, the properties besides the id are nullable to indicate that they may not be updated if they are null.
/// </summary>
public record UserUpdateDTO
{
    public Guid Id { get; init; }
    public string? FirstName { get; init; } = null;
    public string? LastName { get; init; } = null;
    public string? Password { get; init; } = null;
    
    public SpecialistUpdateDTO? Specialist { get; init; } = null;
}
