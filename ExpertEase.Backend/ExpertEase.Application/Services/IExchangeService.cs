using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface IExchangeService
{
    Task<ServiceResponse<UserExchangeDTO>> GetExchange(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<UserExchangeDTO>>> GetExchanges(Guid id, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
}