using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface IAccountService
{
    Task<ServiceResponse> AddAccount(AccountAddDTO account, UserDTO requestingUser, CancellationToken cancellationToken = default);
    Task<ServiceResponse<AccountDTO>> GetAccount(UserDTO requestingUser, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateAccount(AccountUpdateDTO account, UserDTO requestingUser, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteAccount(UserDTO requestingUser, CancellationToken cancellationToken = default);
}