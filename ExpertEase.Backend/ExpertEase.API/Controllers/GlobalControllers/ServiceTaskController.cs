using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Enums;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("/api/[controller]/[action]")]
public class ServiceTaskController(IUserService userService, IServiceTaskService specialistService): AuthorizedController(userService)
{
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ServiceTaskDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await specialistService.GetServiceTask(id)) : 
            CreateErrorMessageResult<ServiceTaskDTO>(currentUser.Error);
    }
    
    // trebuie sa adaug get page aici
    
    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Complete([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        var jobStatus = new JobStatusUpdateDTO
        {
            Id = id,
            Status = JobStatusEnum.Completed
        };
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.UpdateServiceTaskStatus(jobStatus, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Cancel([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        var jobStatus = new JobStatusUpdateDTO
        {
            Id = id,
            Status = JobStatusEnum.Cancelled
        };
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.UpdateServiceTaskStatus(jobStatus, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}