using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Specifications;

public interface ISpecialistService
{
    Task<ServiceResponse> AddSpecialist(SpecialistAddDTO specialist, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}
