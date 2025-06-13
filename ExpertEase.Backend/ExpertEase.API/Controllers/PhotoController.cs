﻿using System.Net;
using ExpertEase.Application.DataTransferObjects.PhotoDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpertEase.API.Controllers;
[ApiController]
[Route("api/[controller]/[action]")]
public class PhotoController(IUserService userService, IPhotoService photoService) : AuthorizedController(userService)
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> AddProfilePicture([FromForm] UploadPhotoDTO photoDTO)
    {
        var currentUser = await GetCurrentUser();

        if (photoDTO.file == null || photoDTO.file.Length == 0)
            return CreateErrorMessageResult(new ErrorMessage(HttpStatusCode.BadRequest, "No file uploaded.", ErrorCodes.CannotAdd));

        var photo = new ProfilePictureAddDTO
        {
            FileStream = photoDTO.file.OpenReadStream(),
            ContentType = photoDTO.file.ContentType,
        };

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await photoService.AddProfilePicture(photo, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpPost]
    public async Task<ActionResult<RequestResponse>> AddPortfolioPicture([FromForm] PortfolioPictureAddDTO photo)
    {
        var currentUser = await GetCurrentUser();
        
        if (currentUser.Result == null)
            return CreateErrorMessageResult(currentUser.Error);

        return CreateRequestResponseFromServiceResponse(await photoService.AddPortfolioPicture(photo, currentUser.Result));
    }
    
    [Authorize]
    [HttpPatch]
    public async Task<ActionResult<RequestResponse>> UpdateProfilePicture([FromForm] UploadPhotoDTO photoDTO)
    {
        var currentUser = await GetCurrentUser();
        
        if (photoDTO.file == null || photoDTO.file.Length == 0)
            return CreateErrorMessageResult(new ErrorMessage(HttpStatusCode.BadRequest, "No file uploaded.", ErrorCodes.CannotAdd));

        var photo = new ProfilePictureAddDTO
        {
            FileStream = photoDTO.file.OpenReadStream(),
            ContentType = photoDTO.file.ContentType,
        };

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await photoService.UpdateProfilePicture(photo, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
    
    [Authorize(Roles = "Specialist")]
    [HttpDelete("{photoId}")]
    public async Task<ActionResult<RequestResponse>> DeletePortfolioPicture([FromRoute] string photoId)
    {
        var currentUser = await GetCurrentUser();

        return currentUser.Result != null ?
            CreateRequestResponseFromServiceResponse(await photoService.DeletePortfolioPicture(photoId, currentUser.Result)) :
            CreateErrorMessageResult(currentUser.Error);
    }
}