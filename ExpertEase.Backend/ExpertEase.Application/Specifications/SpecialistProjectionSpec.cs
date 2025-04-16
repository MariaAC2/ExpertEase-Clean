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
