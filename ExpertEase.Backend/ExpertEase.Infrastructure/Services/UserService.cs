using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Authorization;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories.Interfaces;

namespace ExpertEase.Infrastructure.Services;

public class UserService(
    IRepository<WebAppDatabaseContext> repository,
    ILoginService loginService): IUserService
{
    public async Task<UserDTO?> GetUser(Guid id, CancellationToken cancellationToken = default)
    {
        return await repository.GetAsync(new UserProjectionSpec(id), cancellationToken);
    }

    // public async Task<IEnumerable<UserDTO>> GetUsers(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    // {
    //     var pagedResult = await repository.PageAsync(pagination, new UserProjectionSpec(pagination.Search), cancellationToken);
    //
    //     return pagedResult.Data; // Returning the collection, or return the whole page if needed
    // }

    public async Task<int> GetUserCount(CancellationToken cancellationToken = default)
    {
        return await repository.GetCountAsync<User>(cancellationToken);
    }

    public async Task<LoginResponseDTO?> Login(LoginDTO login, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new UserSpec(login.Email), cancellationToken);

        if (result == null)
        {
            return null;
        }
        
        
        var loginPassword = PasswordUtils.HashPassword(login.Password);

        if (result.Password != loginPassword)
        {
            return null; // Or throw new InvalidPasswordException(); if you want explicit error handling
        }

        var user = new UserDTO
        {
            Id = result.Id,
            Email = result.Email,
            FirstName = result.FirstName,
            LastName = result.LastName,
            Role = result.Role
        };

        return new LoginResponseDTO
        {
            User = user,
            Token = loginService.GetToken(user, DateTime.UtcNow, TimeSpan.FromDays(7))
        };
    }

    public async Task<bool> AddUser(UserAddDTO user, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin)
        {
            return false; // Or throw new UnauthorizedAccessException("Only admins can add users!");
        }

        var existingUser = await repository.GetAsync(new UserSpec(user.Email), cancellationToken);

        if (existingUser != null)
        {
            return false; // Or throw new ConflictException("User already exists!");
        }

        await repository.AddAsync(new User
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Password = PasswordUtils.HashPassword(user.Password)
        }, cancellationToken);

        return true;
    }

    public async Task<bool> UpdateUser(UserUpdateDTO user, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser != null &&
            requestingUser.Role != UserRoleEnum.Admin &&
            requestingUser.Id != user.Id)
        {
            return false; // Or throw new UnauthorizedAccessException("Only admins can update other users");
        }

        var entity = await repository.GetAsync(new UserSpec(user.Id), cancellationToken);

        if (entity == null)
        {
            return false; // Or throw new NotFoundException("User not found");
        }

        entity.FirstName = user.FirstName ?? entity.FirstName;
        entity.LastName = user.LastName ?? entity.LastName;
        entity.Password = user.Password ?? entity.Password;

        await repository.UpdateAsync(entity, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteUser(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser != null &&
            requestingUser.Role != UserRoleEnum.Admin &&
            requestingUser.Id != id)
        {
            return false; // Or throw new UnauthorizedAccessException("Only admins can delete other users");
        }

        await repository.DeleteAsync<User>(id, cancellationToken);

        return true;
    }
}
