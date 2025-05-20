using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Application.DataTransferObjects.TransactionDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface ITransactionService
{
    Task<ServiceResponse<TransactionDTO>> GetTransaction(Specification<Transaction, TransactionDTO> spec, Guid id, CancellationToken cancellationToken = default); 
    Task<ServiceResponse<PagedResponse<TransactionDTO>>> GetTransactions(Specification<Transaction, TransactionDTO> spec, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    public Task<ServiceResponse<int>> GetTransactionCount(CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddTransaction(TransactionAddDTO transaction, UserDTO? requestingUser, CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddTransfer(ServiceTask serviceTask, CancellationToken cancellationToken = default);

    Task<ServiceResponse> UpdateTransaction(TransactionUpdateDTO transaction, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> CancelTransaction(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteTransaction(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}