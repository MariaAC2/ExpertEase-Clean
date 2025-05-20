using System.Net;
using ExpertEase.Application.Constants;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
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

public class SpecialistProfileService(
    IRepository<WebAppDatabaseContext> repository,
    ILoginService loginService,
    IMailService mailService): ISpecialistProfileService
{
    public async Task<ServiceResponse<BecomeSpecialistResponseDTO>> AddSpecialistProfile(BecomeSpecialistDTO becomeSpecialistProfile, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse<BecomeSpecialistResponseDTO>(new(HttpStatusCode.Forbidden, "User cannot become specialist because it doesn't exist!", ErrorCodes.CannotAdd));
        }
        var user = await repository.GetAsync(new UserSpec(requestingUser.Email), cancellationToken);
        if (user == null)
        {
            return ServiceResponse.CreateErrorResponse<BecomeSpecialistResponseDTO>(CommonErrors.UserNotFound);
        }
        
        var existingSpecialist = await repository.GetAsync(new SpecialistSpec(becomeSpecialistProfile.UserId), cancellationToken);
        if (existingSpecialist != null)
        {
            return ServiceResponse.CreateErrorResponse<BecomeSpecialistResponseDTO>(new(HttpStatusCode.Conflict, "The specialist already exists!", ErrorCodes.UserAlreadyExists));
        }
        
        user.Role = UserRoleEnum.Specialist;
        user.RoleString = UserRoleEnum.Specialist.ToString();
        
        if (user.ContactInfo == null)
        {
            user.ContactInfo = new ContactInfo
            {
                UserId = user.Id
            };
        }

        user.ContactInfo.PhoneNumber = becomeSpecialistProfile.PhoneNumber;
        user.ContactInfo.Address = becomeSpecialistProfile.Address;

        user.ContactInfo.PhoneNumber = becomeSpecialistProfile.PhoneNumber;
        user.ContactInfo.Address = becomeSpecialistProfile.Address;

        user.SpecialistProfile = new SpecialistProfile
        {
            UserId = requestingUser.Id,
            YearsExperience = becomeSpecialistProfile.YearsExperience,
            YearsExperienceString = becomeSpecialistProfile.YearsExperience.ToString(),
            Description = becomeSpecialistProfile.Description,
        };

        if (becomeSpecialistProfile.Categories != null)
        {
            var validCategories = new List<Category>();

            foreach (var categoryName in becomeSpecialistProfile.Categories)
            {
                var category = await repository.GetAsync(new CategorySpec(categoryName), cancellationToken);

                if (category == null)
                {
                    return ServiceResponse.CreateErrorResponse<BecomeSpecialistResponseDTO>(new(HttpStatusCode.Conflict,
                        "Cannot add a category that doesn't exist", ErrorCodes.EntityNotFound));
                }

                validCategories.Add(category);
            }

            user.SpecialistProfile.Categories = validCategories;
        }

        var userDTO = new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            ContactInfo = user.ContactInfo != null
                ? new ContactInfoDTO
                {
                    PhoneNumber = user.ContactInfo.PhoneNumber,
                    Address = user.ContactInfo.Address,
                }
                : null,
            Specialist = user.SpecialistProfile != null
                ? new SpecialistProfileDTO
                {
                    YearsExperience = user.SpecialistProfile.YearsExperience,
                    Description = user.SpecialistProfile.Description,
                    Categories = user.SpecialistProfile.Categories.Select(c => new CategoryDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                    }).ToList()
                }
                : null,
        };
        
        await repository.AddAsync(user.SpecialistProfile, cancellationToken);
        await mailService.SendMail(user.Email, "Welcome!", MailTemplates.SpecialistAddTemplate(user.FullName), true, "ExpertEase", cancellationToken); // You can send a notification on the user email. Change the email if you want.
        
        return ServiceResponse.CreateSuccessResponse(new BecomeSpecialistResponseDTO
        {
            Token = loginService.GetToken(userDTO, DateTime.UtcNow, new(7, 0, 0, 0)), // Get a JWT for the user issued now and that expires in 7 days.
            User = userDTO
        });
    }
    
    public async Task<ServiceResponse<SpecialistProfileDTO>> GetSpecialistProfile(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new SpecialistProfileProjectionSpec(userId), cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<SpecialistProfileDTO>(CommonErrors.UserNotFound);
    }

    public async Task<ServiceResponse> UpdateSpecialistProfile(SpecialistProfileUpdateDTO specialistProfile, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && requestingUser.Id != specialistProfile.UserId) // Verify who can add the user, you can change this however you se fit.
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin or the own user can update the user!", ErrorCodes.CannotUpdate));
        }

        var entity = await repository.GetAsync(new UserSpec(specialistProfile.UserId), cancellationToken); 

        if (entity != null)
        {
            entity.ContactInfo.PhoneNumber = specialistProfile.PhoneNumber ?? entity.ContactInfo.PhoneNumber;
            entity.ContactInfo.Address = specialistProfile.Address ?? entity.ContactInfo.Address;
            entity.SpecialistProfile.YearsExperience = specialistProfile.YearsExperience ?? entity.SpecialistProfile.YearsExperience;
            entity.SpecialistProfile.Description = specialistProfile.Description ?? entity.SpecialistProfile.Description;

            await repository.UpdateAsync(entity, cancellationToken);
        }

        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse> DeleteSpecialistProfile(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && requestingUser.Id != id) // Verify who can add the user, you can change this however you se fit.
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin or the own user can delete the user!", ErrorCodes.CannotDelete));
        }

        await repository.DeleteAsync<User>(id, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }
}