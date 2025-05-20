using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Enums;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.SpecialistControllers;

[ApiController]
[Route("/api/specialist/replies/{replyId}/task")]
public class SpecialistServiceTaskController(IUserService userService, IServiceTaskService specialistService): AuthorizedController(userService)
{
    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RequestResponse>> Update([FromRoute] Guid id, [FromBody] ServiceTaskUpdateDTO serviceTask)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.UpdateServiceTask(serviceTask, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}/confirm")]
    public async Task<ActionResult<RequestResponse>> Confirm([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        var jobStatus = new JobStatusUpdateDTO
        {
            Id = id,
            Status = JobStatusEnum.Confirmed
        };
        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await specialistService.UpdateServiceTaskStatus(jobStatus, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpPatch("{id:guid}/cancel")]
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