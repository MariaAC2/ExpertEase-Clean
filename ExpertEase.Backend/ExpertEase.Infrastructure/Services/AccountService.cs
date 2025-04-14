using System.Net;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class AccountService (IRepository<WebAppDatabaseContext> repository): IAccountService
{
    public async Task<ServiceResponse> AddAccount(AccountAddDTO account, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new AccountUserSpec(account.UserId), cancellationToken);
        
        if (result != null)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.Conflict, "The account already was created!", ErrorCodes.AccountAlreadyExists));
        }

        await repository.AddAsync(new Account
            {
                UserId = account.UserId,
                Currency = account.Currency,
                Balance = account.InitialBalance,
            }
            , cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse<AccountDTO>> GetAccount(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new AccountProjectionSpec(id), cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<AccountDTO>(new (HttpStatusCode.NotFound, "Account not found!", ErrorCodes.EntityNotFound));
    }
    
    public async Task<ServiceResponse<AccountDTO>> GetUserAccount(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new AccountUserProjectionSpec(userId), cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<AccountDTO>(new (HttpStatusCode.NotFound, "Account not found!", ErrorCodes.EntityNotFound));
    }
    
    public async Task<ServiceResponse> UpdateAccount(AccountUpdateDTO account, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && requestingUser.Id != account.UserId) // Verify who can add the user, you can change this however you se fit.
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin and own user can update account!", ErrorCodes.CannotUpdate));
        }
        
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && account.Amount != null) // Verify who can add the user, you can change this however you se fit.
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin can update balance!", ErrorCodes.CannotUpdate));
        }
        
        var result = await repository.GetAsync(new AccountUserSpec(account.UserId), cancellationToken);

        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Account not found!", ErrorCodes.EntityNotFound));
        }

        result.Currency = account.Currency ?? result.Currency;

        if (account.Amount.HasValue)
        {
            result.Balance += account.Amount.Value;

            if (result.Balance < 0)
            {
                return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest, "Insufficient funds!", ErrorCodes.CannotUpdate));
            }
        }

        await repository.UpdateAsync(result, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse> DeleteAccount(Guid id, UserDTO? requestingUser, CancellationToken cancellationToken = default)
    {
        if (requestingUser == null || requestingUser.Role != UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.Forbidden,
                "Only admins are allowed to delete accounts!", ErrorCodes.CannotDelete));
        }

        var result = await repository.GetAsync(new AccountSpec(id), cancellationToken);

        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
                "Account doesn't exist!", ErrorCodes.EntityNotFound));
        }

        await repository.DeleteAsync<Account>(result.Id, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }
}