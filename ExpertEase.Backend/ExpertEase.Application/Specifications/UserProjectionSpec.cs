﻿using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
        Query.Include(e => e.SpecialistProfile);
        // Query.Include(e=> e.Specialist.Categories);
        
        Query.Select(e => new UserDTO
        {
            Id = e.Id,
            Email = e.Email,
            FirstName = e.FirstName,
            LastName = e.LastName,
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

    // public UserProjectionSpec(string? search) : this(true)
    // {
    //     Query.Where(s=> s.Role != UserRoleEnum.Specialist);
    //     search = !string.IsNullOrWhiteSpace(search) ? search.Trim() : null;
    //
    //     if (search == null)
    //         return;
    //
    //     var searchExpr = $"%{search.Replace(" ", "%")}%";
    //
    //     Query.Where(e =>
    //         EF.Functions.ILike(e.FirstName, searchExpr) ||
    //         EF.Functions.ILike(e.LastName, searchExpr) ||
    //         EF.Functions.ILike(e.RoleString, searchExpr)
    //     );
    // }
    public UserProjectionSpec(string? search) : this(true)
    {
        Query.Where(s => s.Role != UserRoleEnum.Specialist);

        if (string.IsNullOrWhiteSpace(search))
            return;

        var terms = search.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var term in terms)
        {
            var searchExpr = $"%{term}%";

            Query.Where(e =>
                EF.Functions.ILike(e.FirstName, searchExpr) ||
                EF.Functions.ILike(e.LastName, searchExpr) ||
                EF.Functions.ILike(e.RoleString, searchExpr));
        }
    }
}

public class AdminUserProjectionSpec : UserProjectionSpec
{
    public AdminUserProjectionSpec(Guid id) : base(id) 
    {
        Query.Where(e => e.Role != UserRoleEnum.Specialist);
    }
}