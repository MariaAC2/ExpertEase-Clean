using ExpertEase.Application.DataTransferObjects;

namespace ExpertEase.Application.Services;

public interface IUserService
{
    Task<UserDTO?> GetUser(Guid id, CancellationToken cancellationToken = default);
    // Task<IEnumerable<UserDTO>> GetUsers(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    Task<int> GetUserCount(CancellationToken cancellationToken = default);
    Task<LoginResponseDTO?> Login(LoginDTO login, CancellationToken cancellationToken = default);
    Task<bool> AddUser(UserAddDTO user, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateUser(UserUpdateDTO user, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteUser(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}
