using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class SpecialistProjectionSpec : UserProjectionSpec
{
    public SpecialistProjectionSpec(Guid id) : base() => Query.Where(e => e.Specialist.UserId == id && e.Role == UserRoleEnum.Specialist);
    
    public SpecialistProjectionSpec(string? search) : base(true) 
    {
        Query.Include(s => s.Specialist);
        Query.Where(s=>s.Role == UserRoleEnum.Specialist);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchExpr = $"%{search.Trim().Replace(" ", "%")}%";

            Query.Where(s =>
                EF.Functions.ILike(s.FirstName, searchExpr) ||
                EF.Functions.ILike(s.LastName, searchExpr) ||
                EF.Functions.ILike(s.Email, searchExpr) ||
                EF.Functions.ILike(s.Role.ToString(), searchExpr) ||
                
                EF.Functions.ILike(s.Specialist.PhoneNumber, searchExpr) ||
                EF.Functions.ILike(s.Specialist.Address, searchExpr) ||
                EF.Functions.ILike(s.Specialist.Description, searchExpr) ||
                EF.Functions.ILike(s.Specialist.YearsExperience.ToString(), searchExpr)
            );
        }
    }
}

public class CategoriesProjectionSpec : Specification<Specialist, CategoriesDTO>
{
    public CategoriesProjectionSpec(Guid userId, bool orderByName = false)
    {
        if (orderByName)
        {
            Query.OrderBy(s => s.Categories.First().Name); // Before Select
        }
        
        Query.Where(s => s.UserId == userId);
        Query.Include(s => s.Categories);

        Query.Select<Specialist, CategoriesDTO>(s => new CategoriesDTO
        {
            Categories = s.Categories
                .OrderBy(c => c.Name) // Order inside the projection
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
        });
    }
    
    public CategoriesProjectionSpec(Guid userId, string? search, bool orderByName = false)
    {
        Query.Where(s => s.UserId == userId);
        Query.Include(s => s.Categories);

        // Normalize and prepare search term
        search = !string.IsNullOrWhiteSpace(search) ? search.Trim().ToLower() : null;

        Query.Select<Specialist, CategoriesDTO>(s => new CategoriesDTO
        {
            Categories = s.Categories
                .Where(c => search == null || 
                            c.Name.ToLower().Contains(search) || 
                            c.Description.ToLower().Contains(search))
                .OrderBy(c => orderByName ? c.Name : "")
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
        });
    }
}