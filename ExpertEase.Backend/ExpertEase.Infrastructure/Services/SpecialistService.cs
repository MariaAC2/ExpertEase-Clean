using System.Net;
using ExpertEase.Application.Constants;
using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
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

public class SpecialistService(
    IRepository<WebAppDatabaseContext> repository,
    IMailService mailService): ISpecialistService
{
    public async Task<ServiceResponse> AddSpecialist(SpecialistAddDTO specialist, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User cannot become specialist because it doesn't exist!", ErrorCodes.CannotAdd));
        }
        var user = await repository.GetAsync(new UserSpec(requestingUser.Email), cancellationToken);
        if (user == null)
        {
            return ServiceResponse.CreateErrorResponse(CommonErrors.UserNotFound);
        }
        
        var existingSpecialist = await repository.GetAsync(new SpecialistSpec(specialist.UserId), cancellationToken);
        if (existingSpecialist != null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Conflict, "The specialist already exists!", ErrorCodes.UserAlreadyExists));
        }

        user.Role = UserRoleEnum.Specialist;
        
        await repository.AddAsync(new Specialist
            {
                UserId = requestingUser.Id,
                PhoneNumber = specialist.PhoneNumber,
                Address = specialist.Address,
                YearsExperience = specialist.YearsExperience,
                Description = specialist.Description,
            }
            , cancellationToken);
        
        var fullName = $"{user.LastName} {user.FirstName}";
        await mailService.SendMail(user.Email, "Welcome!", MailTemplates.SpecialistAddTemplate(fullName), true, "ExpertEase", cancellationToken); // You can send a notification on the user email. Change the email if you want.
        
        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse<SpecialistDTO>> GetSpecialist(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new SpecialistProjectionSpec(userId), cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<SpecialistDTO>(CommonErrors.UserNotFound);
    }
    
    public async Task<ServiceResponse<PagedResponse<SpecialistDTO>>> GetSpecialists(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, new SpecialistProjectionSpec(pagination.Search), cancellationToken); // Use the specification and pagination API to get only some entities from the database.

        return ServiceResponse.CreateSuccessResponse(result);
    }

    public async Task<ServiceResponse> UpdateSpecialist(SpecialistUpdateDTO specialist, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && requestingUser.Id != specialist.UserId) // Verify who can add the user, you can change this however you se fit.
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin or the own user can update the user!", ErrorCodes.CannotUpdate));
        }

        var entity = await repository.GetAsync(new SpecialistSpec(specialist.UserId), cancellationToken); 

        if (entity != null)
        {
            entity.PhoneNumber = specialist.PhoneNumber ?? entity.PhoneNumber;
            entity.Address = specialist.Address ?? entity.Address;
            entity.YearsExperience = specialist.YearsExperience ?? entity.YearsExperience;
            entity.Description = specialist.Description ?? entity.Description;

            await repository.UpdateAsync(entity, cancellationToken);
        }

        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse> DeleteSpecialist(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && requestingUser.Id != id) // Verify who can add the user, you can change this however you se fit.
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin or the own user can delete the user!", ErrorCodes.CannotDelete));
        }

        await repository.DeleteAsync<User>(id, cancellationToken); // Delete the entity.

        return ServiceResponse.CreateSuccessResponse();
    }
}