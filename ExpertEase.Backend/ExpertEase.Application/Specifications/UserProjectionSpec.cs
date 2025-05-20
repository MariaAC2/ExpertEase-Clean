using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

/// <summary>
/// This is a specification to filter the user entities and map it to and UserDTO object via the constructors.
/// The specification will project the entity onto a DTO so it isn't tracked by the framework.
/// Note how the constructors call other constructors which can be used to chain them. Also, this is a sealed class, meaning it cannot be further derived.
/// </summary>
public class UserProjectionSpec : Specification<User, UserDTO>
{
    /// <summary>
    /// In this constructor is the projection/mapping expression used to get UserDTO object directly from the database.
    /// </summary>
    public UserProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Include(e => e.Account);
        Query.Include(e => e.SpecialistProfile)
            .ThenInclude(e => e.Categories);
        
        Query.Select(e => new UserDTO
        {
            Id = e.Id,
            Email = e.Email,
            FullName = e.FullName,
            Role = e.Role,
            RoleString = e.RoleString,
            ContactInfo = e.ContactInfo != null
                ? new ContactInfoDTO
                {
                    PhoneNumber = e.ContactInfo.PhoneNumber,
                    Address = e.ContactInfo.Address
                }
                : null,
            Specialist = e.SpecialistProfile != null
                ? new SpecialistProfileDTO
                {
                    YearsExperience = e.SpecialistProfile.YearsExperience,
                    Description = e.SpecialistProfile.Description,
                    Categories = e.SpecialistProfile.Categories.Select(c => new CategoryDTO
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Description = c.Description,
                        }).ToList()
                }
                : null
        });

        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }

    public UserProjectionSpec(Guid id) : this()
    {
        Query.Where(e => e.Id == id);
    }
}

public class AdminUserProjectionSpec: Specification<User, UserDTO>
{
    public AdminUserProjectionSpec(Guid adminId, bool orderByCreatedAt = false)
    {
        Query.Include(e => e.Account);
        Query.Include(e => e.SpecialistProfile)
            .ThenInclude(e => e.Categories);
        
        Query.Where(e => e.Id != adminId && e.Role != UserRoleEnum.Specialist);
        Query.Select(e => new UserDTO
        {
            Id = e.Id,
            Email = e.Email,
            FullName = e.FullName,
            Role = e.Role,
            RoleString = e.RoleString,
            ContactInfo = e.ContactInfo != null
                ? new ContactInfoDTO
                {
                    PhoneNumber = e.ContactInfo.PhoneNumber,
                    Address = e.ContactInfo.Address
                }
                : null,
            Specialist = e.SpecialistProfile != null
                ? new SpecialistProfileDTO
                {
                    YearsExperience = e.SpecialistProfile.YearsExperience,
                    Description = e.SpecialistProfile.Description,
                    Categories = e.SpecialistProfile.Categories.Select(c => new CategoryDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                    }).ToList()
                }
                : null
        });

        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }
    public AdminUserProjectionSpec(Guid id, Guid adminId) : this(adminId)
    {
        Query.Where(e => e.Id == id);
    }
    
    public AdminUserProjectionSpec(string? search, Guid adminId) : this(adminId, true)
    {
        if (string.IsNullOrWhiteSpace(search))
            return;

        var terms = search.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var term in terms)
        {
            var searchExpr = $"%{term}%";

            Query.Where(e =>
                EF.Functions.ILike(e.FullName, searchExpr) ||
                EF.Functions.ILike(e.RoleString, searchExpr));
        }
    }
}