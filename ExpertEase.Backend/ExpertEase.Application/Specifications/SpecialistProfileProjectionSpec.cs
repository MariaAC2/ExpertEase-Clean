using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpertEase.Application.Specifications;

public class SpecialistProfileProjectionSpec : Specification<User, SpecialistProfileDTO>
{
    public SpecialistProfileProjectionSpec(Guid id)
    {
        Query.Where(e => e.SpecialistProfile != null && e.SpecialistProfile.UserId == id && e.Role == UserRoleEnum.Specialist);
        Query.Include(e => e.SpecialistProfile)
            .ThenInclude(e => e.Categories);
        Query.Select(e => new SpecialistProfileDTO
        {
            YearsExperience = e.SpecialistProfile.YearsExperience,
            Description = e.SpecialistProfile.Description,
            Categories = e.SpecialistProfile.Categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name
            }).ToList()
        });
    }
}

public class StripeAccountIdProjectionSpec : Specification<User, string>
{
    public StripeAccountIdProjectionSpec(Guid id)
    {
        Query.Where(e => e.Id == id);
        Query.Select(e => e.SpecialistProfile.StripeAccountId ?? string.Empty);
    }
}
