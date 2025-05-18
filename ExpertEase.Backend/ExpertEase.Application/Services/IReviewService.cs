using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Application.DataTransferObjects.ReviewDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface IReviewService
{
    Task<ServiceResponse<ReviewDTO>> GetReview(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<ServiceResponse<ReviewAdminDTO>> GetReviewAdmin(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<ReviewDTO>>> GetReviews(Guid userId, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<ReviewAdminDTO>>> GetReviewsAdmin(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    // public Task<ServiceResponse<int>> GetRequestCount(CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddReview(ReviewAddDTO review, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateRequest(ReviewUpdateDTO review, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteReview(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}