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
    public async Task<ServiceResponse> AddAccount(AccountAddDTO account, UserDTO? requestingUser, CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User cannot add account because it doesn't exist!", ErrorCodes.CannotAdd));
        }
        
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
}