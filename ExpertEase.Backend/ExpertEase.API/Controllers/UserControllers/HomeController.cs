using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers.UserControllers;

[ApiController]
[Route("api/home")]
public class HomeController(ISpecialistService specialistService) : ResponseController
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RequestResponse<SpecialistDTO>>> GetById([FromRoute] Guid id)
    {
        return CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialist(id));
    }

    [HttpGet]
    public async Task<ActionResult<RequestResponse<PagedResponse<SpecialistDTO>>>> GetPage([FromQuery] PaginationSearchQueryParams pagination)
    {
        return CreateRequestResponseFromServiceResponse(await specialistService.GetSpecialists(pagination));
    }
}