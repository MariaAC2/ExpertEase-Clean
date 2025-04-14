using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

/// <summary>
/// This is a specification to filter the user entities and map it to and UserDTO object via the constructors.
/// The specification will project the entity onto a DTO so it isn't tracked by the framework.
/// Note how the constructors call other constructors which can be used to chain them. Also, this is a sealed class, meaning it cannot be further derived.
/// </summary>
public sealed class UserProjectionSpec : Specification<User, UserDTO>
{
    /// <summary>
    /// In this constructor is the projection/mapping expression used to get UserDTO object directly from the database.
    /// </summary>
    public UserProjectionSpec(bool orderByCreatedAt = false)
    {
        // Include the related entities first
        Query.Include(e => e.Account);
        Query.Include(e => e.Specialist);

        // Select into full UserDTO
        Query.Select(e => new UserDTO
        {
            Id = e.Id,
            Email = e.Email,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Role = e.Role,
            Account = e.Account != null
                ? new AccountDTO
                {
                    Id = e.Account.Id,
                    Balance = e.Account.Balance
                }
                : null,
            Specialist = e.Specialist != null
                ? new SpecialistOnlyDTO
                {
                    PhoneNumber = e.Specialist.PhoneNumber,
                    Address = e.Specialist.Address,
                    YearsExperience = e.Specialist.YearsExperience,
                    Description = e.Specialist.Description
                }
                : null
        });

        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }

    public UserProjectionSpec(Guid id) : this() => Query.Where(e => e.Id == id); // This constructor will call the first declared constructor with the default parameter.

    public UserProjectionSpec(string? search) : this(true) // This constructor will call the first declared constructor with 'true' as the parameter. 
    {
        search = !string.IsNullOrWhiteSpace(search) ? search.Trim() : null;
    
        if (search == null)
        {
            return;
        }
    
        var searchExpr = $"%{search.Replace(" ", "%")}%";
    
        Query.Where(e => EF.Functions.ILike(e.LastName, searchExpr)); // This is an example on how database specific expressions can be used via C# expressions.
                                                                                          // Note that this will be translated to the database something like "where user.Name ilike '%str%'".
    }
}
