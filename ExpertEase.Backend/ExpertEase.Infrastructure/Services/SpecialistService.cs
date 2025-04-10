using System.Net;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class SpecialistService(IRepository<WebAppDatabaseContext> repository): ISpecialistService
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

        // Step 2: Check if already a specialist
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
                City = specialist.City,
                Country = specialist.Country,
                YearsExperience = specialist.YearsExperience,
                Description = specialist.Description,
            }
            , cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }
}