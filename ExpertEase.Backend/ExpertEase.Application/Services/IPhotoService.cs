﻿using ExpertEase.Application.DataTransferObjects.PhotoDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Responses;

namespace ExpertEase.Application.Services;

public interface IPhotoService
{
    private Task<string> AddPhoto(PhotoAddDTO photo, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    private Task DeletePhoto<ServiceResponse>(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    
    public Task<ServiceResponse> AddProfilePicture(ProfilePictureAddDTO photo, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default);

    public Task<ServiceResponse> AddPortfolioPicture(PortfolioPictureAddDTO photo, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default);

    public Task<ServiceResponse> UpdateProfilePicture(ProfilePictureAddDTO photoDto, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default);

    public Task<ServiceResponse> DeletePortfolioPicture(string photoId, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default);
}