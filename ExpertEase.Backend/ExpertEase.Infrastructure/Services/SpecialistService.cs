using System.Net;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class SpecialistService(IRepository<WebAppDatabaseContext> repository) : ISpecialistService
{
    public async Task<ServiceResponse> AddSpecialist(SpecialistAddDTO user, UserDTO? requestingUser, CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin) // Verify who can add the user, you can change this however you se fit.
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin can add users!", ErrorCodes.CannotAdd));
        }

        var result = await repository.GetAsync(new UserSpec(user.Email), cancellationToken);

        if (result != null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Conflict, "The user already exists!", ErrorCodes.UserAlreadyExists));
        }

        var newUser = new User
        {
            Email = user.Email,
            FullName = user.FullName,
            Role = UserRoleEnum.Specialist,
            RoleString = UserRoleEnum.Specialist.ToString(),
            Password = user.Password,
            ContactInfo = new ContactInfo
            {
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            },
            SpecialistProfile = new SpecialistProfile
            {
                YearsExperience = user.YearsExperience,
                YearsExperienceString = user.YearsExperience.ToString(),
                Description = user.Description,
                Categories = new List<Category>()
            }
        };
        
        await repository.AddAsync(newUser, cancellationToken);
        
        newUser.Account = new Account
        {
            UserId = newUser.Id,
            Currency = "RON",
            Balance = 0
        };
            
        await repository.AddAsync(newUser.Account, cancellationToken);
        await repository.UpdateAsync(newUser, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse<SpecialistDTO>> GetSpecialist(Guid id, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new SpecialistProjectionSpec(id), cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<SpecialistDTO>(CommonErrors.UserNotFound);
    }
    
    public async Task<ServiceResponse<PagedResponse<SpecialistDTO>>> GetSpecialists(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, new SpecialistProjectionSpec(pagination.Search), cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse> UpdateSpecialist(SpecialistUpdateDTO user, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin can update users!", ErrorCodes.CannotAdd));
        }

        var result = await repository.GetAsync(new UserSpec(user.Id), cancellationToken);

        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "The user doesn't exist!", ErrorCodes.EntityNotFound));
        }

        // Safely update fields with null-checks
        result.FullName = user.FullName ?? result.FullName;

        if (result.ContactInfo != null)
        {
            result.ContactInfo.PhoneNumber = user.PhoneNumber ?? result.ContactInfo.PhoneNumber;
            result.ContactInfo.Address = user.Address ?? result.ContactInfo.Address;
        }

        if (result.SpecialistProfile != null)
        {
            result.SpecialistProfile.YearsExperience = user.YearsExperience ?? result.SpecialistProfile.YearsExperience;
            result.SpecialistProfile.Description = user.Description ?? result.SpecialistProfile.Description;
        }

        await repository.UpdateAsync(result, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }
}