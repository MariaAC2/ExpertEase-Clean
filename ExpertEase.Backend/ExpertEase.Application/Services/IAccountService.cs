using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface IAccountService
{
    Task<ServiceResponse> AddAccount(AccountAddDTO account, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}