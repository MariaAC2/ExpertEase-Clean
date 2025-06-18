using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.LoginDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface IUserService
{
    Task<ServiceResponse<UserDTO>> GetUser(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResponse<UserDTO>> GetUserAdmin(Guid id, Guid adminId, CancellationToken cancellationToken = default);
    Task<ServiceResponse<UserPaymentDetailsDTO>> GetUserPaymentDetails(Guid id,
        CancellationToken cancellationToken = default);
    Task<ServiceResponse<UserDetailsDTO>> GetUserDetails(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<PagedResponse<UserDTO>>> GetUsers(Guid adminId, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse<int>> GetUserCount(CancellationToken cancellationToken = default);
    Task<ServiceResponse<LoginResponseDTO>> Login(LoginDTO login, CancellationToken cancellationToken = default);

    Task<ServiceResponse<LoginResponseDTO>> SocialLogin(SocialLoginDTO loginDto,
        CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddUser(UserAddDTO user, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);

    Task<ServiceResponse<UserUpdateResponseDTO>> UpdateUser(UserUpdateDTO user, UserDTO? requestingUser,
        CancellationToken cancellationToken = default);
    Task<ServiceResponse> AdminUpdateUser(AdminUserUpdateDTO user, UserDTO? requestingUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResponse> DeleteUser(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}
