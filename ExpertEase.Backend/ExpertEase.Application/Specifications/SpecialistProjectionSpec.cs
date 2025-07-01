using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
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
            YearsExperience = e.SpecialistProfile != null ? e.SpecialistProfile.YearsExperience : 0,
            Description = e.SpecialistProfile != null ? e.SpecialistProfile.Description : "",
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            Rating = e.Rating,
            Categories = e.SpecialistProfile != null
                ? e.SpecialistProfile.Categories.Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
                : new List<CategoryDTO>()
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
    
    public SpecialistProjectionSpec(SpecialistPaginationQueryParams searchParams) : this(true)
    {
        Query.Where(e => e.Role == UserRoleEnum.Specialist);
        
        // Apply category filters
        ApplyCategoryFilters(searchParams.Filter.CategoryId, searchParams.Filter.CategoryName);
        
        // Apply rating filters
        ApplyRatingFilters(searchParams.Filter.MinRating, searchParams.Filter.MaxRating);
        
        // Apply experience range filter
        ApplyExperienceRangeFilter(searchParams.Filter.ExperienceRange);
        
        // Apply rating sorting
        ApplyRatingSorting(searchParams.Filter.SortByRating);
    }
    
    public SpecialistProjectionSpec(PaginationSearchQueryParams searchParams) : this(true)
    {
        searchParams.Search = !string.IsNullOrWhiteSpace(searchParams.Search) ? searchParams.Search.Trim() : null;

        if (searchParams.Search == null)
            return;

        var searchExpr = $"%{searchParams.Search.Replace(" ", "%")}%";

        Query.Where(e =>
            EF.Functions.ILike(e.FullName, searchExpr) ||
            EF.Functions.ILike(e.Email, searchExpr) ||
            (e.ContactInfo != null && EF.Functions.ILike(e.ContactInfo.PhoneNumber, searchExpr)) ||
            (e.ContactInfo != null && EF.Functions.ILike(e.ContactInfo.Address, searchExpr)) ||
            (e.SpecialistProfile != null && EF.Functions.ILike(e.SpecialistProfile.Description, searchExpr))
        );
    }
    
    private void ApplyCategoryFilters(Guid? categoryId, string? categoryName)
    {
        if (categoryId.HasValue)
        {
            Query.Where(e => e.SpecialistProfile != null && 
                           e.SpecialistProfile.Categories.Any(c => c.Id == categoryId.Value));
        }
        
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            var categoryNameExpr = $"%{categoryName.Trim()}%";
            Query.Where(e => e.SpecialistProfile != null && 
                           e.SpecialistProfile.Categories.Any(c => EF.Functions.ILike(c.Name, categoryNameExpr)));
        }
    }
    
    private void ApplyRatingFilters(int? minRating, int? maxRating)
    {
        if (minRating.HasValue)
        {
            Query.Where(e => e.Rating >= minRating.Value);
        }
        
        if (maxRating.HasValue)
        {
            Query.Where(e => e.Rating <= maxRating.Value);
        }
    }
    
    private void ApplyRatingSorting(string? sortByRating)
    {
        if (string.IsNullOrWhiteSpace(sortByRating))
            return;
            
        switch (sortByRating.ToLowerInvariant())
        {
            case "asc":
                Query.OrderBy(e => e.Rating);
                break;
            case "desc":
                Query.OrderByDescending(e => e.Rating);
                break;
        }
    }
    
    private void ApplyExperienceRangeFilter(string? experienceRange)
    {
        if (string.IsNullOrWhiteSpace(experienceRange))
            return;
            
        switch (experienceRange.ToLowerInvariant())
        {
            case "0-2":
                Query.Where(e => e.SpecialistProfile != null && 
                               e.SpecialistProfile.YearsExperience >= 0 && 
                               e.SpecialistProfile.YearsExperience <= 2);
                break;
            case "2-5":
                Query.Where(e => e.SpecialistProfile != null && 
                               e.SpecialistProfile.YearsExperience > 2 && 
                               e.SpecialistProfile.YearsExperience <= 5);
                break;
            case "5-7":
                Query.Where(e => e.SpecialistProfile != null && 
                               e.SpecialistProfile.YearsExperience > 5 && 
                               e.SpecialistProfile.YearsExperience <= 7);
                break;
            case "7-10":
                Query.Where(e => e.SpecialistProfile != null && 
                               e.SpecialistProfile.YearsExperience > 7 && 
                               e.SpecialistProfile.YearsExperience <= 10);
                break;
            case "10+":
                Query.Where(e => e.SpecialistProfile != null && 
                               e.SpecialistProfile.YearsExperience > 10);
                break;
        }
    }
}