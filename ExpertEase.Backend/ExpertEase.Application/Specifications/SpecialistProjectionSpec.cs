﻿using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class SpecialistProjectionSpec: Specification<User, SpecialistDTO>
{
    public SpecialistProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Include(e => e.ContactInfo);
        Query.Include(e => e.SpecialistProfile)
            .ThenInclude(e => e.Categories);
        Query.Select(e => new SpecialistDTO
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                ProfilePictureUrl = e.ProfilePictureUrl,
                PhoneNumber = e.ContactInfo != null ? e.ContactInfo.PhoneNumber : "",
                Address = e.ContactInfo != null ? e.ContactInfo.Address : "",
                YearsExperience = e.SpecialistProfile.YearsExperience,
                Description = e.SpecialistProfile.Description,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                Rating = e.Rating,
                Categories = e.SpecialistProfile.Categories.Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                }).ToList()
            });
        
        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.CreatedAt);
        }
    }
    
    public SpecialistProjectionSpec(Guid id) : this()
    {
        Query.Where(e => e.Id == id && e.Role == UserRoleEnum.Specialist);
    }
    
    public SpecialistProjectionSpec(string? search) : this(true)
    {
        Query.Where(e => e.Role == UserRoleEnum.Specialist);
        search = !string.IsNullOrWhiteSpace(search) ? search.Trim() : null;

        if (search == null)
            return;

        var searchExpr = $"%{search.Replace(" ", "%")}%";

        Query.Where(e =>
            EF.Functions.ILike(e.FullName, searchExpr) ||
            EF.Functions.ILike(e.Email, searchExpr) ||
            EF.Functions.ILike(e.ContactInfo.PhoneNumber, searchExpr) ||
            EF.Functions.ILike(e.ContactInfo.Address, searchExpr)
            // EF.Functions.ILike(
            //     string.Join(",", e.Specialist.Categories.Select(c => c.Name)), 
            //     searchExpr
            // )
        );
    }
}