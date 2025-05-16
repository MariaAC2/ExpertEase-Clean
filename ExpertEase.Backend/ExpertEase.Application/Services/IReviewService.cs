using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.ReviewDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public class IReviewServices
{
    // Task<ServiceResponse<RequestDTO>> GetRequest(Specification<Request, RequestDTO> spec, CancellationToken cancellationToken = default);
    // Task<ServiceResponse<PagedResponse<RequestDTO>>> GetRequests(Specification<Request, RequestDTO> spec, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    // public Task<ServiceResponse<int>> GetRequestCount(CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddReview(ReviewAddDTO review, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    // Task<ServiceResponse> UpdateRequest(RequestUpdateDTO request, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    // Task<ServiceResponse> UpdateRequestStatus(StatusUpdateDTO request, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    // Task<ServiceResponse> DeleteRequest(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}