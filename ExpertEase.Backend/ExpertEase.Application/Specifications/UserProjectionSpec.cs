using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.ReviewDTOs;
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
    private UserProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Include(e => e.SpecialistProfile)
            .ThenInclude(e => e.Categories);
        
        Query.Select(e => new UserDTO
        {
            Id = e.Id,
            Email = e.Email,
            FullName = e.FullName,
            Role = e.Role,
            AuthProvider = e.AuthProvider,
            StripeCustomerId = e.StripeCustomerId,
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

public class UserPaymentDetailsProjectionSpec : Specification<User, UserPaymentDetailsDTO>
{
    public UserPaymentDetailsProjectionSpec(Guid userId)
    {
        Query
            .Where(u => u.Id == userId)
            .Include(u => u.ContactInfo);
        Query.Select(u => new UserPaymentDetailsDTO 
        {
            UserId = u.Id,
            UserFullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.ContactInfo!.PhoneNumber,
        });
    }
}

public class AdminUserProjectionSpec: Specification<User, UserDTO>
{
    public AdminUserProjectionSpec(Guid adminId, bool orderByCreatedAt = false)
    {
        Query.Include(e => e.SpecialistProfile)
            .ThenInclude(e => e.Categories);
        
        Query.Where(e => e.Id != adminId && e.Role != UserRoleEnum.Specialist);
        Query.Select(e => new UserDTO
        {
            Id = e.Id,
            Email = e.Email,
            FullName = e.FullName,
            Role = e.Role,
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
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchExpr = $"%{search.Trim().Replace(" ", "%")}%";
            Query.Where(e =>
                EF.Functions.ILike(e.FullName, searchExpr));
        }
    }
}

public class UserDetailsProjectionSpec : Specification<User, UserDetailsDTO>
{
    public UserDetailsProjectionSpec(Guid userId)
    {
        Query
            .Where(u => u.Id == userId)
            .Include(u => u.ContactInfo)
            .Include(u => u.SpecialistProfile!)
                .ThenInclude(sp => sp.Categories);
        Query.Select(u => new UserDetailsDTO
            {
                FullName = u.FullName,
                ProfilePictureUrl = u.ProfilePictureUrl,
                Rating = u.Rating,

                // Include these only for specialists
                Email = u.Role == UserRoleEnum.Specialist ? u.Email : null,
                PhoneNumber = u.Role == UserRoleEnum.Specialist ? u.ContactInfo!.PhoneNumber : null,
                Address = u.Role == UserRoleEnum.Specialist ? u.ContactInfo!.Address : null,
                YearsExperience = u.Role == UserRoleEnum.Specialist ? u.SpecialistProfile!.YearsExperience : null,
                Description = u.Role == UserRoleEnum.Specialist ? u.SpecialistProfile!.Description : null,
                Portfolio = u.Role == UserRoleEnum.Specialist ? 
                    u.SpecialistProfile!.Portfolio.ToList() 
                    : null,
                Categories = u.Role == UserRoleEnum.Specialist
                    ? u.SpecialistProfile!.Categories.Select(c => c.Name).ToList()
                    : null
            });
    }
}

public class UserProfileProjectionSpec : Specification<User, UserProfileDTO>
{
    public UserProfileProjectionSpec(Guid userId)
    {
        Query
            .Where(u => u.Id == userId)
            .Include(u => u.ContactInfo)
            .Include(u => u.SpecialistProfile!)
            .ThenInclude(sp => sp.Categories);
        Query.Select(u => new UserProfileDTO
        {
            Id = u.Id,
            FullName = u.FullName,
            ProfilePictureUrl = u.ProfilePictureUrl,
            Rating = u.Rating,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,

            // Include these only for specialists
            Email = u.Email,
            PhoneNumber = u.ContactInfo!.PhoneNumber,
            Address = u.ContactInfo!.Address,
            YearsExperience = u.Role == UserRoleEnum.Specialist ? u.SpecialistProfile!.YearsExperience : null,
            Description = u.Role == UserRoleEnum.Specialist ? u.SpecialistProfile!.Description : null,
            StripeAccountId = u.Role == UserRoleEnum.Specialist ? u.SpecialistProfile!.StripeAccountId : null,
            Portfolio = u.Role == UserRoleEnum.Specialist ? 
                u.SpecialistProfile!.Portfolio.ToList() 
                : null,
            Categories = u.Role == UserRoleEnum.Specialist
                ? u.SpecialistProfile!.Categories.Select(c => c.Name).ToList()
                : null
        });
    }
}