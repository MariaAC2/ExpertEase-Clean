using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface ISpecialistService
{
    Task<ServiceResponse> AddSpecialist(SpecialistAddDTO user, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse<SpecialistDTO>> GetSpecialist(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<SpecialistDTO>>> GetSpecialists(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateSpecialist(SpecialistUpdateDTO user, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteSpecialist(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}