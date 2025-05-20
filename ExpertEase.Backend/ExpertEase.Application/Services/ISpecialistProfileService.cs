using ExpertEase.Application.DataTransferObjects.SpecialistDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface ISpecialistProfileService
{
    Task<ServiceResponse<BecomeSpecialistResponseDTO>> AddSpecialistProfile(BecomeSpecialistDTO becomeSpecialistProfile, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse<SpecialistProfileDTO>> GetSpecialistProfile(Guid userId, CancellationToken cancellationToken = default); 
    Task<ServiceResponse> UpdateSpecialistProfile(SpecialistProfileUpdateDTO specialistProfile, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteSpecialistProfile(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}
