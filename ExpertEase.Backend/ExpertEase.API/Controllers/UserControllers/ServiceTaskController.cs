using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("/api/user/replies/{replyId}/task")]
public class ServiceTaskController(IUserService userService, IServiceTaskService specialistService): AuthorizedController(userService)
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<ServiceTaskDTO>>> GetById([FromRoute] Guid id)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ? 
            CreateRequestResponseFromServiceResponse(await specialistService.GetServiceTask(id)) : 
            CreateErrorMessageResult<ServiceTaskDTO>(currentUser.Error);
    }
}