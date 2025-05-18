using System.Net;
using ExpertEase.Application.Constants;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.DataTransferObjects.CategoryDTOs;
using ExpertEase.Application.DataTransferObjects.LoginDTOs;
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

public class UserService(
    IRepository<WebAppDatabaseContext> repository,
    ILoginService loginService,
    IMailService mailService): IUserService
{
    public async Task<ServiceResponse<UserDTO>> GetUser(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new UserProjectionSpec(id), cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<UserDTO>(CommonErrors.UserNotFound);
    }
    
    public async Task<ServiceResponse<UserDTO>> GetUserAdmin(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new AdminUserProjectionSpec(id), cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<UserDTO>(CommonErrors.UserNotFound);
    }
    
    public async Task<ServiceResponse<PagedResponse<UserDTO>>> GetUsers(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, new UserProjectionSpec(pagination.Search), cancellationToken); // Use the specification and pagination API to get only some entities from the database.

        return ServiceResponse.CreateSuccessResponse(result);
    }

    public async Task<ServiceResponse<int>> GetUserCount(CancellationToken cancellationToken = default) => 
        ServiceResponse.CreateSuccessResponse(await repository.GetCountAsync<User>(cancellationToken));

    public async Task<ServiceResponse<LoginResponseDTO>> Login(LoginDTO login, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new UserSpec(login.Email), cancellationToken);

        if (result == null) // Verify if the user is found in the database.
        {
            return ServiceResponse.CreateErrorResponse<LoginResponseDTO>(CommonErrors.UserNotFound); // Pack the proper error as the response.
        }

        if (result.Password != login.Password) // Verify if the password hash of the request is the same as the one in the database.
        {
            return ServiceResponse.CreateErrorResponse<LoginResponseDTO>(new(HttpStatusCode.BadRequest, "Wrong password!", ErrorCodes.WrongPassword));
        }

        var user = new UserDTO
        {
            Id = result.Id,
            Email = result.Email,
            FirstName = result.FirstName,
            LastName = result.LastName,
            Role = result.Role,
            ContactInfo = result.ContactInfo != null
                ? new ContactInfoDTO
                {
                    PhoneNumber = result.ContactInfo.PhoneNumber,
                    Address = result.ContactInfo.Address,
                } : null,
            Specialist = result.SpecialistProfile != null
                ? new SpecialistProfileDTO
                {
                    YearsExperience = result.SpecialistProfile.YearsExperience,
                    Description = result.SpecialistProfile.Description,
                    Categories = result.SpecialistProfile.Categories.Select(c => new CategoryDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                    }).ToList()
                } : null,
        };

        return ServiceResponse.CreateSuccessResponse(new LoginResponseDTO
        {
            User = user,
            Token = loginService.GetToken(user, DateTime.UtcNow, new(7, 0, 0, 0)) // Get a JWT for the user issued now and that expires in 7 days.
        });
    }
    public async Task<ServiceResponse> AddUser(UserAddDTO user, UserDTO? requestingUser, CancellationToken cancellationToken = default)
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
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            RoleString = user.Role.ToString(),
            Password = user.Password
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
        
        // var fullName = $"{user.LastName} {user.FirstName}";
        // await mailService.SendMail(user.Email, "Welcome!", MailTemplates.UserAddTemplate(fullName), true, "ExpertEase Team", cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse> UpdateUser(UserUpdateDTO user, UserDTO? requestingUser, CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && requestingUser.Id != user.Id) // Verify who can add the user, you can change this however you se fit.
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin or the own user can update the user!", ErrorCodes.CannotUpdate));
        }

        var entity = await repository.GetAsync(new UserSpec(user.Id), cancellationToken); 

        if (entity == null)
        {
            return ServiceResponse.CreateErrorResponse(
                new(HttpStatusCode.NotFound, "User not found", ErrorCodes.EntityNotFound));
        }
        
        entity.FirstName = user.FirstName ?? entity.FirstName;
        entity.LastName = user.LastName ?? entity.LastName;
        entity.Password = user.Password ?? entity.Password;

        await repository.UpdateAsync(entity, cancellationToken); // Update the entity and persist the changes.

        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse> DeleteUser(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && requestingUser.Id != id) // Verify who can add the user, you can change this however you se fit.
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin or the own user can delete the user!", ErrorCodes.CannotDelete));
        }

        await repository.DeleteAsync<User>(id, cancellationToken); // Delete the entity.

        return ServiceResponse.CreateSuccessResponse();
    }
    
}
