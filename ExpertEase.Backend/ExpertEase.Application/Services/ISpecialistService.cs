using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface ISpecialistService
{
    Task<ServiceResponse> AddSpecialist(SpecialistAddDTO user, UserDTO? requestingUser, CancellationToken cancellationToken = default);
    Task<ServiceResponse<SpecialistDTO>> GetSpecialist(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<SpecialistDTO>>> GetSpecialists(SpecialistPaginationQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<SpecialistDTO>>> SearchSpecialistsByCategory(Guid categoryId, PaginationQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<SpecialistDTO>>> SearchSpecialistsByRatingRange(int minRating, int maxRating, PaginationQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<SpecialistDTO>>> GetTopRatedSpecialists(PaginationQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<SpecialistDTO>>> SearchSpecialistsByExperienceRange(string experienceRange, PaginationQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateSpecialist(SpecialistUpdateDTO user, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteSpecialist(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}