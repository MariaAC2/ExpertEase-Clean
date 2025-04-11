namespace ExpertEase.Application.DataTransferObjects;

/// <summary>
/// This DTO is used to update a user, the properties besides the id are nullable to indicate that they may not be updated if they are null.
/// </summary>
public record UserUpdateDTO(Guid Id, string? FirstName = null, string? LastName = null, string? Password = null);
