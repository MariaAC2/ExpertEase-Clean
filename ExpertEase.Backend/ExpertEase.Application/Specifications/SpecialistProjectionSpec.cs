using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
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
        search = !string.IsNullOrWhiteSpace(search) ? search.Trim() : null;
    
        if (search == null)
        {
            return;
        }
    
        var searchExpr = $"%{search.Replace(" ", "%")}%";
    
        Query.Where(e => EF.Functions.ILike(e.User.LastName, searchExpr));
    }

}