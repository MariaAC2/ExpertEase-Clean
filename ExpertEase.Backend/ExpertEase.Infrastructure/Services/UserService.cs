﻿using System.Net;
using System.Text.Json;
using ExpertEase.Application.DataTransferObjects.LoginDTOs;
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
using Google.Apis.Auth;
using Stripe;

namespace ExpertEase.Infrastructure.Services;

public class UserService(
    IRepository<WebAppDatabaseContext> repository,
    ILoginService loginService,
    IMailService mailService,
    HttpClient httpClient,
    IStripeAccountService stripeAccountService) : IUserService
{
    public async Task<ServiceResponse<UserDTO>> GetUser(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new UserProjectionSpec(id), cancellationToken);

        return result != null
            ? ServiceResponse.CreateSuccessResponse(result)
            : ServiceResponse.CreateErrorResponse<UserDTO>(CommonErrors.UserNotFound);
    }
    
    public async Task<ServiceResponse<UserDetailsDTO>> GetUserDetails(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new UserDetailsProjectionSpec(id), cancellationToken);
        
        if (result == null)
            return ServiceResponse.CreateErrorResponse<UserDetailsDTO>(CommonErrors.UserNotFound);
        
        result.Reviews = await repository.ListAsync(new ReviewProjectionSpec(id), cancellationToken);

        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse<UserProfileDTO>> GetUserProfile(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new UserProfileProjectionSpec(id), cancellationToken);
        
        if (result == null)
            return ServiceResponse.CreateErrorResponse<UserProfileDTO>(CommonErrors.UserNotFound);

        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse<UserPaymentDetailsDTO>> GetUserPaymentDetails(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new UserPaymentDetailsProjectionSpec(id), cancellationToken);
        
        return result == null ? 
            ServiceResponse.CreateErrorResponse<UserPaymentDetailsDTO>(CommonErrors.UserNotFound) : 
            ServiceResponse.CreateSuccessResponse(result);
    }

    public async Task<ServiceResponse<UserDTO>> GetUserAdmin(Guid id, Guid adminId,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new AdminUserProjectionSpec(id, adminId), cancellationToken);

        return result != null
            ? ServiceResponse.CreateSuccessResponse(result)
            : ServiceResponse.CreateErrorResponse<UserDTO>(CommonErrors.UserNotFound);
    }

    public async Task<ServiceResponse<PagedResponse<UserDTO>>> GetUsers(Guid adminId,
        PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, new AdminUserProjectionSpec(pagination.Search, adminId),
            cancellationToken); // Use the specification and pagination API to get only some entities from the database.

        return ServiceResponse.CreateSuccessResponse(result);
    }

    public async Task<ServiceResponse<int>> GetUserCount(CancellationToken cancellationToken = default)
    {
        return ServiceResponse.CreateSuccessResponse(await repository.GetCountAsync<User>(cancellationToken));
    }

    public async Task<ServiceResponse<LoginResponseDTO>> Login(LoginDTO login,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new UserSpec(login.Email), cancellationToken);

        if (result == null) // Verify if the user is found in the database.
            return ServiceResponse.CreateErrorResponse<LoginResponseDTO>(CommonErrors
                .UserNotFound); // Pack the proper error as the response.

        if (result.Password !=
            login.Password) // Verify if the password hash of the request is the same as the one in the database.
            return ServiceResponse.CreateErrorResponse<LoginResponseDTO>(new ErrorMessage(HttpStatusCode.BadRequest,
                "Wrong password!", ErrorCodes.WrongPassword));

        var user = new UserDTO
        {
            Id = result.Id,
            Email = result.Email,
            FullName = result.FullName,
            Role = result.Role,
            AuthProvider = result.AuthProvider,
        };

        return ServiceResponse.CreateSuccessResponse(new LoginResponseDTO
        {
            User = user,
            Token = loginService.GetToken(user, DateTime.UtcNow,
                new TimeSpan(7, 0, 0, 0)) // Get a JWT for the user issued now and that expires in 7 days.
        });
    }

    public async Task<ServiceResponse<LoginResponseDTO>> SocialLogin(SocialLoginDTO loginDto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(loginDto.Token))
            return ServiceResponse.CreateErrorResponse<LoginResponseDTO>(
                new ErrorMessage(HttpStatusCode.BadRequest, "Missing token", ErrorCodes.Invalid));

        var provider = loginDto.Provider.ToLower();
        SocialUserInfo? userInfo = null;

        try
        {
            userInfo = provider switch
            {
                "google" => await ValidateGoogleToken(loginDto.Token),
                "facebook" => await ValidateFacebookToken(loginDto.Token),
                _ => null
            };
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse<LoginResponseDTO>(
                new ErrorMessage(HttpStatusCode.BadRequest, $"Invalid {provider} token: {ex.Message}", ErrorCodes.Invalid));
        }

        if (userInfo == null)
            return ServiceResponse.CreateErrorResponse<LoginResponseDTO>(
                new ErrorMessage(HttpStatusCode.BadRequest, "Unsupported provider or invalid token", ErrorCodes.Invalid));

        var result = await repository.GetAsync(new UserSpec(userInfo.Email), cancellationToken);

        if (result == null)
        {
            var authProvider = provider switch
            {
                "google" => AuthProvider.Google,
                "facebook" => AuthProvider.Facebook,
                _ => AuthProvider.Local
            };

            var newUser = new User
            {
                Email = userInfo.Email,
                FullName = userInfo.Name,
                Role = userInfo.Email.EndsWith("@admin.com", StringComparison.OrdinalIgnoreCase)
                    ? UserRoleEnum.Admin
                    : UserRoleEnum.Client,
                AuthProvider = authProvider,
                ProfilePictureUrl = userInfo.Picture
            };

            await repository.AddAsync(newUser, cancellationToken);
            result = newUser;
        }
        
        var user = new UserDTO
        {
            Id = result.Id,
            Email = result.Email,
            FullName = result.FullName,
            Role = result.Role,
            AuthProvider = result.AuthProvider,
        };
        
        return ServiceResponse.CreateSuccessResponse(new LoginResponseDTO
        {
            User = user,
            Token = loginService.GetToken(user, DateTime.UtcNow, new TimeSpan(7, 0, 0, 0))
        });
    }

    private async Task<SocialUserInfo?> ValidateGoogleToken(string token)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(token);
        return new SocialUserInfo
        {
            Email = payload.Email,
            Name = payload.Name,
            Picture = payload.Picture
        };
    }

    private async Task<SocialUserInfo?> ValidateFacebookToken(string token)
    {
        // First, verify the token and get user info from Facebook Graph API
        var response = await httpClient.GetAsync($"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={token}");
        
        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to validate Facebook token");

        var content = await response.Content.ReadAsStringAsync();
        var facebookUser = JsonSerializer.Deserialize<FacebookUserResponse>(content, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        if (facebookUser == null || string.IsNullOrEmpty(facebookUser.Email))
            throw new Exception("Invalid Facebook user data");

        return new SocialUserInfo
        {
            Email = facebookUser.Email,
            Name = facebookUser.Name,
            Picture = facebookUser.Picture?.Data?.Url
        };
    }

    public async Task<ServiceResponse> AddUser(UserAddDTO user, UserDTO? requestingUser,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null &&
            requestingUser.Role !=
            UserRoleEnum.Admin) // Verify who can add the user, you can change this however you se fit.
            return ServiceResponse.CreateErrorResponse(new ErrorMessage(HttpStatusCode.Forbidden,
                "Only the admin can add users!",
                ErrorCodes.CannotAdd));

        var result = await repository.GetAsync(new UserSpec(user.Email), cancellationToken);

        if (result != null)
            return ServiceResponse.CreateErrorResponse(new ErrorMessage(HttpStatusCode.Conflict,
                "The user already exists!",
                ErrorCodes.UserAlreadyExists));

        var newUser = new User
        {
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            Password = user.Password
        };

        await repository.AddAsync(newUser, cancellationToken);
        
        var stripeCustomerId = await stripeAccountService.CreateCustomer(newUser.Email, newUser.FullName, newUser.Id);
        Console.WriteLine("Stripe customer id: " + stripeCustomerId.Result);

        if (stripeCustomerId.Result == null)
        {
            return ServiceResponse.CreateErrorResponse(new ErrorMessage(HttpStatusCode.Forbidden,
                "Stripe customer id doesn't exist"));
        }
        
        // Update user with Stripe customer ID
        newUser.StripeCustomerId = stripeCustomerId.Result;
        await repository.UpdateAsync(newUser, cancellationToken);

        // var fullName = $"{user.LastName} {user.FirstName}";
        // await mailService.SendMail(user.Email, "Welcome!", MailTemplates.UserAddTemplate(fullName), true, "ExpertEase Team", cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse<UserUpdateResponseDTO>> UpdateUser(UserUpdateDTO user, UserDTO? requestingUser,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin &&
            requestingUser.Id != user.Id) // Verify who can add the user, you can change this however you se fit.
            return ServiceResponse.CreateErrorResponse<UserUpdateResponseDTO>(new ErrorMessage(HttpStatusCode.Forbidden,
                "Only the admin or the own user can update the user!", ErrorCodes.CannotUpdate));

        var entity = await repository.GetAsync(new UserSpec(user.Id), cancellationToken);

        if (entity == null)
            return ServiceResponse.CreateErrorResponse<UserUpdateResponseDTO>(
                new ErrorMessage(HttpStatusCode.NotFound, "User not found", ErrorCodes.EntityNotFound));

        if (!string.IsNullOrWhiteSpace(entity.FullName))
        {
            var nameParts = entity.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : user.FirstName;
            var lastName = nameParts.Length > 1 ? nameParts[1] : user.LastName;
            entity.FullName = $"{firstName} {lastName}";
        }

        entity.Password = user.Password ?? entity.Password;
        entity.ContactInfo ??= new ContactInfo
        {
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Address = user.Address ?? string.Empty
        };

        await repository.UpdateAsync(entity, cancellationToken); // Update the entity and persist the changes.
        
        var userDto = new UserDTO
        {
            Id = entity.Id,
            Email = entity.Email,
            FullName = entity.FullName,
            Role = entity.Role,
            AuthProvider = entity.AuthProvider,
        };

        return ServiceResponse.CreateSuccessResponse(new UserUpdateResponseDTO
        {
            User = user,
            Token = loginService.GetToken(userDto, DateTime.UtcNow,
                new TimeSpan(7, 0, 0, 0)) // Get a JWT for the user issued now and that expires in 7 days.
        });
    }

    public async Task<ServiceResponse> AdminUpdateUser(AdminUserUpdateDTO user, UserDTO? requestingUser,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin &&
            requestingUser.Id != user.Id) // Verify who can add the user, you can change this however you se fit.
            return ServiceResponse.CreateErrorResponse(new ErrorMessage(HttpStatusCode.Forbidden,
                "Only the admin or the own user can update the user!", ErrorCodes.CannotUpdate));

        var entity = await repository.GetAsync(new UserSpec(user.Id), cancellationToken);

        if (entity == null)
            return ServiceResponse.CreateErrorResponse(
                new ErrorMessage(HttpStatusCode.NotFound, "User not found", ErrorCodes.EntityNotFound));

        entity.FullName = user.FullName ?? entity.FullName;
        entity.Role = user.Role ?? entity.Role;

        await repository.UpdateAsync(entity, cancellationToken); // Update the entity and persist the changes.

        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse> DeleteUser(Guid id, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin &&
            requestingUser.Id != id) // Verify who can add the user, you can change this however you se fit.
            return ServiceResponse.CreateErrorResponse(new ErrorMessage(HttpStatusCode.Forbidden,
                "Only the admin or the own user can delete the user!", ErrorCodes.CannotDelete));

        await repository.DeleteAsync<User>(id, cancellationToken); // Delete the entity.

        return ServiceResponse.CreateSuccessResponse();
    }
}