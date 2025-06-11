using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface IServiceTaskService
{
    Task<ServiceResponse<ServiceTask>> AddServiceTask(ServiceTaskAddDTO service, CancellationToken cancellationToken = default);
    Task<ServiceResponse<ServiceTaskDTO>> GetServiceTask(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<ServiceTaskDTO>>> GetServiceTasks(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateServiceTask(ServiceTaskUpdateDTO serviceTask, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateServiceTaskStatus(JobStatusUpdateDTO serviceTask, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteServiceTask(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
}