using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class SpecialistProjectionSpec : Specification<Specialist, SpecialistDTO>
{
    public SpecialistProjectionSpec(bool orderByCreatedAt = false)
    {
        Query.Include(e => e.User);
        
        Query.Select(e => new SpecialistDTO
        {
            PhoneNumber = e.PhoneNumber,
            Address = e.Address,
            YearsExperience = e.YearsExperience,
            Description = e.Description,

            User = new UserDTO
            {
                Id = e.User.Id,
                Email = e.User.Email,
                FirstName = e.User.FirstName,
                LastName = e.User.LastName,
                Role = e.User.Role,

                Account = e.User.Account != null
                    ? new AccountDTO
                    {
                        Id = e.User.Account.Id,
                        Balance = e.User.Account.Balance
                    }
                    : null
            }
        });
        if (orderByCreatedAt)
        {
            Query.OrderByDescending(e => e.User.CreatedAt);
        }
    }
    
    public SpecialistProjectionSpec(Guid id) : this() => Query.Where(e => e.UserId == id);
    
    public SpecialistProjectionSpec(string? search) : this(true) // This constructor will call the first declared constructor with 'true' as the parameter. 
    {
        Query.Include(s => s.User); // Include user for joined field access

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchExpr = $"%{search.Trim().Replace(" ", "%")}%";

            Query.Where(s =>
                EF.Functions.ILike(s.User.FirstName, searchExpr) ||
                EF.Functions.ILike(s.User.LastName, searchExpr) ||
                EF.Functions.ILike(s.User.Email, searchExpr) ||
                EF.Functions.ILike(s.User.Role.ToString(), searchExpr) ||
                
                EF.Functions.ILike(s.PhoneNumber, searchExpr) ||
                EF.Functions.ILike(s.Address, searchExpr) ||
                EF.Functions.ILike(s.Description, searchExpr) ||
                EF.Functions.ILike(s.YearsExperience.ToString(), searchExpr)
            );
        }
    }

}