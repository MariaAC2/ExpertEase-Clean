using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface IRequestService
{
    Task<ServiceResponse<RequestDTO>> GetRequest(Guid requestId, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<RequestDTO>>> GetRequests(Specification<Request, RequestDTO> spec, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    public Task<ServiceResponse<int>> GetRequestCount(CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddRequest(RequestAddDTO request, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateRequest(RequestUpdateDTO request, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateRequestStatus(StatusUpdateDTO request, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteRequest(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}