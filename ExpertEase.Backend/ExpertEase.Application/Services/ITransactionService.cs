using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.TransactionDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface ITransactionService
{
    Task<ServiceResponse<TransactionDTO>> GetTransaction(Guid id, CancellationToken cancellationToken = default); 
    Task<ServiceResponse<PagedResponse<TransactionDTO>>> GetTransactions(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<TransactionDTO>>> GetTransactionsByUser(Guid userId, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    public Task<ServiceResponse<int>> GetTransactionCount(CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddTransaction(TransactionAddDTO user, UserDTO? requestingUser, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateTransaction(TransactionUpdateDTO user, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> CancelTransaction(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteTransaction(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}