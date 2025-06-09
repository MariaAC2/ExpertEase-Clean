using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface IAccountService
{
    Task<ServiceResponse> AddAccount(AccountAddDTO account, CancellationToken cancellationToken = default);
    Task<ServiceResponse<AccountDTO>> GetAccount(Guid userId, CancellationToken cancellationToken = default);
    Task<ServiceResponse<AccountDTO>> GetUserAccount(Guid userId, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateAccount(AccountUpdateDTO account, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteAccount(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}