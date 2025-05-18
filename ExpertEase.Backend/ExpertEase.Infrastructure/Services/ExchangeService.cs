using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class ExchangeService(IRepository<WebAppDatabaseContext> repository): IExchangeService
{
    public async Task<ServiceResponse<UserExchangeDTO>> GetExchange(Guid currentUserId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);
        
        if (user.Role == UserRoleEnum.Client) 
        {
            var result = await repository.GetAsync(new ExchangeUserProjectionSpec(userId), cancellationToken);
            return result != null ? 
                ServiceResponse.CreateSuccessResponse(result) : 
                ServiceResponse.CreateErrorResponse<UserExchangeDTO>(CommonErrors.EntityNotFound);
        } else if (user.Role == UserRoleEnum.Specialist) 
        {
            var result = await repository.GetAsync(new ExchangeSpecialistProjectionSpec(userId), cancellationToken);
            return result != null ? 
                ServiceResponse.CreateSuccessResponse(result) : 
                ServiceResponse.CreateErrorResponse<UserExchangeDTO>(CommonErrors.EntityNotFound);
        } else 
        {
            return ServiceResponse.CreateErrorResponse<UserExchangeDTO>(CommonErrors.NotAllowed);
        }
    }

    public async Task<ServiceResponse<PagedResponse<UserExchangeDTO>>> GetExchanges(Guid currentUserId,
        PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetAsync(new UserSpec(currentUserId), cancellationToken);
        
        if (user.Role == UserRoleEnum.Client) 
        {
            var result = await repository.PageAsync(pagination, new ExchangeUserProjectionSpec(pagination.Search), cancellationToken);
            return ServiceResponse.CreateSuccessResponse(result);
        } else if (user.Role == UserRoleEnum.Specialist) 
        {
            var result = await repository.PageAsync(pagination, new ExchangeSpecialistProjectionSpec(pagination.Search), cancellationToken);
            return ServiceResponse.CreateSuccessResponse(result);
        } else 
        {
            return ServiceResponse.CreateErrorResponse<PagedResponse<UserExchangeDTO>>(CommonErrors.NotAllowed);
        }
    }
}