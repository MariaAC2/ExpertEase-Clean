using System.Net;
using ExpertEase.Application.Constants;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.PhotoDTOs;
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
    IStripeAccountService stripeAccountService,
    ILoginService loginService,
    IPhotoService photoService,
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
        
        var existingSpecialist = await repository.GetAsync(new SpecialistProfileSpec(becomeSpecialistProfile.UserId), cancellationToken);
        if (existingSpecialist != null)
        {
            return ServiceResponse.CreateErrorResponse<BecomeSpecialistResponseDTO>(new(HttpStatusCode.Conflict, "The specialist already exists!", ErrorCodes.UserAlreadyExists));
        }
        
        user.Role = UserRoleEnum.Specialist;
        UserRoleEnum.Specialist.ToString();
        
        if (user.ContactInfo == null)
        {
            user.ContactInfo = new ContactInfo
            {
                UserId = user.Id,
                PhoneNumber = becomeSpecialistProfile.PhoneNumber,
                Address = becomeSpecialistProfile.Address
            };
            
            await repository.AddAsync(user.ContactInfo, cancellationToken);
        } else 
        {
            user.ContactInfo.PhoneNumber = becomeSpecialistProfile.PhoneNumber;
            user.ContactInfo.Address = becomeSpecialistProfile.Address;
            await repository.UpdateAsync(user.ContactInfo, cancellationToken);
        }

        user.SpecialistProfile = new SpecialistProfile
        {
            UserId = requestingUser.Id,
            YearsExperience = becomeSpecialistProfile.YearsExperience,
            Description = becomeSpecialistProfile.Description,
        };
        
        var stripeAccountId = await stripeAccountService.CreateConnectedAccount(user.Email);
        user.SpecialistProfile.StripeAccountId = stripeAccountId;

        if (becomeSpecialistProfile.Categories != null)
        {
            var validCategories = new List<Category>();

            foreach (var categoryId in becomeSpecialistProfile.Categories)
            {
                var category = await repository.GetAsync(new CategorySpec(categoryId), cancellationToken);

                if (category == null)
                {
                    return ServiceResponse.CreateErrorResponse<BecomeSpecialistResponseDTO>(new(HttpStatusCode.Conflict,
                        "Cannot add a category that doesn't exist", ErrorCodes.EntityNotFound));
                }

                validCategories.Add(category);
            }

            user.SpecialistProfile.Categories = validCategories;
        }
        
        if (becomeSpecialistProfile.PortfolioPhotos != null)
        {
            var validPhotos = new List<string?>();

            foreach (var photoDTO in becomeSpecialistProfile.PortfolioPhotos)
            {
                var urlResponse = await photoService.AddPortfolioPicture(photoDTO, requestingUser, cancellationToken);

                validPhotos.Add(urlResponse.ToString());
            }

            user.SpecialistProfile.Portfolio = validPhotos;
        }

        var userDTO = new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
        };
        
        await repository.AddAsync(user.SpecialistProfile, cancellationToken);
        await repository.UpdateAsync(user, cancellationToken);
        // await mailService.SendMail(user.Email, "Welcome!", MailTemplates.SpecialistAddTemplate(user.FullName), true, "ExpertEase", cancellationToken); // You can send a notification on the user email. Change the email if you want.
        
        return ServiceResponse.CreateSuccessResponse(new BecomeSpecialistResponseDTO
        {
            Token = loginService.GetToken(userDTO, DateTime.UtcNow, new(7, 0, 0, 0)), // Get a JWT for the user issued now and that expires in 7 days.
            User = userDTO,
            StripeAccountId = stripeAccountId
        });
    }
    
    public async Task<ServiceResponse<SpecialistProfileDTO>> GetSpecialistProfile(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new SpecialistProfileProjectionSpec(userId), cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<SpecialistProfileDTO>(CommonErrors.UserNotFound);
    }

    public async Task<ServiceResponse> UpdateSpecialistProfile(
        SpecialistProfileUpdateDTO specialistProfile, 
        List<PortfolioPictureAddDTO>? newPhotos = null,
        UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && requestingUser.Id != specialistProfile.UserId)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin or the own user can update the user!", ErrorCodes.CannotUpdate));
        }

        var entity = await repository.GetAsync(new UserSpec(specialistProfile.UserId), cancellationToken);

        if (entity?.SpecialistProfile == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Specialist profile not found!", ErrorCodes.EntityNotFound));
        }

        // Update basic profile information
        if (entity.ContactInfo != null)
        {
            entity.ContactInfo.PhoneNumber = specialistProfile.PhoneNumber ?? entity.ContactInfo.PhoneNumber;
            entity.ContactInfo.Address = specialistProfile.Address ?? entity.ContactInfo.Address;
        }
        
        entity.SpecialistProfile.YearsExperience = specialistProfile.YearsExperience ?? entity.SpecialistProfile.YearsExperience;
        entity.SpecialistProfile.Description = specialistProfile.Description ?? entity.SpecialistProfile.Description;

        // Handle photo management
        var currentPortfolio = entity.SpecialistProfile.Portfolio ?? new List<string>();
        var updatedPortfolio = new List<string>();

        try
        {
            // 1. Keep existing photos that are not being removed
            if (specialistProfile.ExistingPortfolioPhotoUrls != null)
            {
                updatedPortfolio.AddRange(specialistProfile.ExistingPortfolioPhotoUrls);
            }

            // 2. Remove photos that are explicitly marked for removal
            if (specialistProfile.PhotoIdsToRemove != null && specialistProfile.PhotoIdsToRemove.Any())
            {
                foreach (var photoIdToRemove in specialistProfile.PhotoIdsToRemove)
                {
                    var deleteResult = await photoService.DeletePortfolioPicture(photoIdToRemove, requestingUser, cancellationToken);
                    if (!deleteResult.IsSuccess)
                    {
                        // Log the error but continue with other operations
                        Console.WriteLine($"Failed to delete photo {photoIdToRemove}: {deleteResult.Error?.Message}");
                    }
                }
            }

            // 3. Upload and add new photos
            if (newPhotos != null && newPhotos.Any())
            {
                foreach (var photoDTO in newPhotos)
                {
                    try
                    {
                        var uploadResult = await photoService.AddPortfolioPicture(photoDTO, requestingUser, cancellationToken);
                        
                        if (uploadResult.IsSuccess)
                        {
                            // Extract URL from the response (you might need to adjust this based on your AddPortfolioPicture return type)
                            var photoUrl = uploadResult.ToString();
                            if (!string.IsNullOrEmpty(photoUrl))
                            {
                                updatedPortfolio.Add(photoUrl);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to upload new photo: {uploadResult.Error?.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception uploading photo: {ex.Message}");
                    }
                    finally
                    {
                        // Ensure streams are disposed
                        photoDTO.FileStream?.Dispose();
                    }
                }
            }

            // 4. Update the portfolio with the new list
            entity.SpecialistProfile.Portfolio = updatedPortfolio;

            // 5. Save changes
            await repository.UpdateAsync(entity, cancellationToken);

            return ServiceResponse.CreateSuccessResponse();
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.InternalServerError, $"Error updating profile: {ex.Message}", ErrorCodes.CannotUpdate));
        }
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