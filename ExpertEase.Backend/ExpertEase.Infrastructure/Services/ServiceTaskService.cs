using System.Net;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class ServiceTaskService(IRepository<WebAppDatabaseContext> repository, 
    IReviewService reviewService): IServiceTaskService
{
    public async Task<ServiceResponse> AddServiceTask(Reply lastReply, CancellationToken cancellationToken = default)
    {
        var sender = await repository.GetAsync(new UserSpec(lastReply.Request.SenderUserId), cancellationToken);
        
        if (sender == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this id not found!", ErrorCodes.EntityNotFound));

        var receiver = await repository.GetAsync(new UserSpec(lastReply.Request.ReceiverUserId), cancellationToken);
        
        if (receiver == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this id not found!", ErrorCodes.EntityNotFound));
        
        var serviceTask = new ServiceTask 
        {
            UserId = sender.Id,
            SpecialistId = receiver.Id,
            ReplyId = lastReply.Id,
            StartDate = lastReply.StartDate,
            EndDate = lastReply.EndDate,
            Address = lastReply.Request.Address,
            Description = lastReply.Request.Description,
            Price = lastReply.Price,
            Status = JobStatusEnum.Confirmed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await repository.AddAsync(serviceTask, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse<ServiceTaskDTO>> GetServiceTask(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new ServiceTaskProjectionSpec(id), cancellationToken);
        
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<ServiceTaskDTO>(CommonErrors.EntityNotFound);
    }
    
    public async Task<ServiceResponse<PagedResponse<ServiceTaskDTO>>> GetServiceTasks(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, new ServiceTaskProjectionSpec(pagination.Search), cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse> UpdateServiceTask(ServiceTaskUpdateDTO serviceTask, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetAsync(new ServiceTaskSpec(serviceTask.Id), cancellationToken);
        
        if (task == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Service task with this id not found!", ErrorCodes.EntityNotFound));
        
        task.StartDate = serviceTask.StartDate ?? task.StartDate;
        task.EndDate = serviceTask.EndDate ?? task.EndDate;
        task.Address = serviceTask.Address ?? task.Address;
        task.Price = serviceTask.Price ?? task.Price;

        await repository.UpdateAsync(task, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse> UpdateServiceTaskStatus(JobStatusUpdateDTO serviceTask, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetAsync(new ServiceTaskSpec(serviceTask.Id), cancellationToken);
        
        if (task == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Service task with this id not found!", ErrorCodes.EntityNotFound));

        if (serviceTask.Status == JobStatusEnum.Completed)
        {
            // create and send review
            // create transfer
            task.CompletedAt = DateTime.UtcNow;
            task.Status = JobStatusEnum.Completed;
            // var addTransferResult = await transactionService.AddTransfer(task, cancellationToken);
            //
            // if (!addTransferResult.IsOk)
            // {
            //     return addTransferResult;
            // }
        }
        else if (serviceTask.Status == JobStatusEnum.Cancelled)
        {
            task.CancelledAt = DateTime.UtcNow;
            task.Status = JobStatusEnum.Cancelled;
        }

        await repository.UpdateAsync(task, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse> DeleteServiceTask(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetAsync(new ServiceTaskSpec(id), cancellationToken);
        
        if (task == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Service task with this id not found!", ErrorCodes.EntityNotFound));

        await repository.DeleteAsync<ServiceTask>(id, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }
}