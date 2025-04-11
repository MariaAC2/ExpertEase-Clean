using System.Net;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class AccountService (IRepository<WebAppDatabaseContext> repository): IAccountService
{
    public async Task<ServiceResponse> AddAccount(AccountAddDTO account, UserDTO requestingUser, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new AccountSpec(requestingUser.Id), cancellationToken);
        
        if (result != null)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.Conflict, "The account already was created!", ErrorCodes.AccountAlreadyExists));
        }

        await repository.AddAsync(new Account
            {
                UserId = requestingUser.Id,
                Balance = account.InitialBalance,
            }
            , cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse<AccountDTO>> GetAccount(UserDTO requestingUser, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new AccountSpec(requestingUser.Id), cancellationToken);
        
        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(CommonErrors.AccountNotFound);
        }

        if (requestingUser.Id != result.UserId)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.Forbidden, "You are not allowed to access this account!", ErrorCodes.WrongUser));
        }

        return ServiceResponse.CreateSuccessResponse(new AccountDTO
        {
            Id = result.Id,
            Balance = result.Balance
        });
    }
    
    public async Task<ServiceResponse> UpdateAccount(AccountUpdateDTO account, UserDTO requestingUser, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new AccountSpec(requestingUser.Id), cancellationToken);
        
        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(CommonErrors.AccountNotFound);
        }

        if (requestingUser.Id != result.UserId)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.Forbidden, "You are not allowed to access this account!", ErrorCodes.WrongUser));
        }

        result.Balance += account.Amount;

        if (result.Balance < 0)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(new (HttpStatusCode.BadRequest, "Insufficient funds!", ErrorCodes.CannotUpdate));
        }

        await repository.UpdateAsync(result, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse> DeleteAccount(UserDTO requestingUser,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new AccountSpec(requestingUser.Id), cancellationToken);
        
        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(CommonErrors.AccountNotFound);
        }

        if (requestingUser.Id != result.UserId)
        {
            return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.Forbidden, "You are not allowed to access this account!", ErrorCodes.WrongUser));
        }
        
        await repository.DeleteAsync<Account>(result.Id, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }
}