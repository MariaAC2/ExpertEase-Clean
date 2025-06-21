using Microsoft.AspNetCore.Http;

namespace ExpertEase.Application.DataTransferObjects.PhotoDTOs;

public class UploadPhotoDTO
{
    public IFormFile? File;
}