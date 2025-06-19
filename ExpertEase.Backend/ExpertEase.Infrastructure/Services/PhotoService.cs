using System.Net;
using ExpertEase.Application.DataTransferObjects.FirestoreDTOs;
using ExpertEase.Application.DataTransferObjects.PhotoDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Firebase.FirestoreMappers;
using ExpertEase.Infrastructure.Firebase.FirestoreRepository;
using ExpertEase.Infrastructure.Firestore.FirestoreDTOs;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class PhotoService(IRepository<WebAppDatabaseContext> repository, 
    IFirestoreRepository firestoreRepository, 
    IFirebaseStorageService firebaseStorageService,
    IConversationService conversationService): IPhotoService
{
    // Maximum file size for conversation photos (e.g., 10MB)
    private const long MaxConversationPhotoSize = 10 * 1024 * 1024;
    
    // Allowed content types for conversation photos
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"
    ];
    private async Task<string> AddPhoto(PhotoAddDTO photo, CancellationToken cancellationToken = default)
    {
        var url = await firebaseStorageService.UploadImageAsync(photo.FileStream, photo.Folder, photo.FileName, photo.ContentType);

        long sizeInBytes = 0;
        if (photo.FileStream.CanSeek)
        {
            sizeInBytes = photo.FileStream.Length;
        }

        var domainPhoto = new Photo
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(photo.UserId),
            Url = url,
            ContentType = photo.ContentType,
            FileName = photo.FileName,
            SizeInBytes = sizeInBytes,
            IsProfilePicture = photo.IsProfilePicture,
            CreatedAt = DateTime.UtcNow
        };

        var firestoreDto = PhotoMapper.ToFirestoreDTO(domainPhoto);
        await firestoreRepository.AddAsync("photos", firestoreDto, cancellationToken);
        return firestoreDto.Url;
    }

    private async Task<ServiceResponse> DeletePhoto(string id, CancellationToken cancellationToken = default)
    {
        var photo = await firestoreRepository.GetAsync<FirestorePhotoDTO>("photos", id, cancellationToken);
        if (photo == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Photo not found"));
        }

        var objectName = new Uri(photo.Url).AbsolutePath.TrimStart('/');

        await firebaseStorageService.DeleteImageAsync(objectName, cancellationToken);
        await firestoreRepository.DeleteAsync<FirestorePhotoDTO>("photos", id, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse> AddProfilePicture(ProfilePictureAddDTO photo, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));
        }

        var photoAddDto = new PhotoAddDTO
        {
            UserId = requestingUser.Id.ToString(),
            FileStream = photo.FileStream,
            Folder = "profile_pictures",
            FileName = requestingUser.Id.ToString(),
            ContentType = photo.ContentType,
            IsProfilePicture = true
        };

        var photoUrl = await AddPhoto(photoAddDto, cancellationToken);

        var user = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
        if (user == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User not found", ErrorCodes.EntityNotFound));
        }

        user.ProfilePictureUrl = photoUrl;
        await repository.UpdateAsync(user, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse> AddPortfolioPicture(PortfolioPictureAddDTO photo, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));
        }

        var photoAddDto = new PhotoAddDTO
        {
            UserId = requestingUser.Id.ToString(),
            FileStream = photo.FileStream,
            Folder = "portfolio_pictures/" + requestingUser.Id,
            FileName = photo.FileName,
            ContentType = photo.ContentType,
            IsProfilePicture = false
        };

        var photoUrl = await AddPhoto(photoAddDto, cancellationToken);

        var user = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
        if (user == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User not found", ErrorCodes.EntityNotFound));
        }

        if (user.Role != UserRoleEnum.Specialist || user.SpecialistProfile == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only specialists can have portfolio pictures", ErrorCodes.CannotAdd));
        }

        // user.SpecialistProfile.Portfolio.Add(photoUrl);
        // await repository.UpdateAsync(user, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse> UpdateProfilePicture(ProfilePictureAddDTO photo, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));

        var existingPhotos = await firestoreRepository.ListAsync<FirestorePhotoDTO>(
            "photos",
            col => col.WhereEqualTo("UserId", requestingUser.Id.ToString()).WhereEqualTo("IsProfilePicture", true),
            cancellationToken
        );

        if (existingPhotos.Any())
        {
            var oldPhoto = existingPhotos.First();
            await DeletePhoto(oldPhoto.Id, cancellationToken);
        }

        var photoDto = new PhotoAddDTO
        {
            UserId = requestingUser.Id.ToString(),
            FileStream = photo.FileStream,
            Folder = "profile_pictures",
            FileName = requestingUser.Id.ToString(),
            ContentType = photo.ContentType,
            IsProfilePicture = true
        };

        var newUrl = await AddPhoto(photoDto, cancellationToken);

        var user = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
        if (user == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User not found"));

        user.ProfilePictureUrl = newUrl;
        await repository.UpdateAsync(user, cancellationToken);

        return ServiceResponse.CreateSuccessResponse(new { profileImageUrl = newUrl });
    }

    public async Task<ServiceResponse> DeletePortfolioPicture(string photoId, UserDTO? requestingUser = null, CancellationToken cancellationToken = default)
    {
        var photo = await firestoreRepository.GetAsync<FirestorePhotoDTO>("photos", photoId, cancellationToken);
        if (photo == null || photo.UserId != requestingUser.Id.ToString())
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Photo not found or unauthorized"));

        var objectName = new Uri(photo.Url).AbsolutePath.TrimStart('/');
        await firebaseStorageService.DeleteImageAsync(objectName, cancellationToken);

        await firestoreRepository.DeleteAsync<FirestorePhotoDTO>("photos", photoId, cancellationToken);

        // var user = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
        // if (user?.SpecialistProfile?.Portfolio != null)
        // {
        //     user.SpecialistProfile.Portfolio.Remove(photo.Url);
        //     await repository.UpdateAsync(user, cancellationToken);
        // }

        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse> ValidateConversationPhoto(
        Stream fileStream, 
        string contentType, 
        long fileSize)
    {
        // Check file size
        if (fileSize > MaxConversationPhotoSize)
        {
            return ServiceResponse.CreateErrorResponse(
                new ErrorMessage(HttpStatusCode.BadRequest, 
                    $"File size exceeds maximum allowed size of {MaxConversationPhotoSize / (1024 * 1024)}MB"));
        }

        // Check content type
        if (!AllowedContentTypes.Contains(contentType.ToLower()))
        {
            return ServiceResponse.CreateErrorResponse(
                new ErrorMessage(HttpStatusCode.BadRequest, 
                    $"Unsupported file type. Allowed types: {string.Join(", ", AllowedContentTypes)}"));
        }

        // Additional validation can be added here (e.g., image dimensions, file content validation)
        
        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse> AddPhotoToConversation(
        Guid conversationId,
        ConversationPhotoUploadDTO photoUpload,
        UserDTO? sender,
        string? caption = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Validate photo
            var validationResult = await ValidateConversationPhoto(
                photoUpload.FileStream, 
                photoUpload.ContentType, 
                photoUpload.FileStream.Length);
            if (!validationResult.IsOk)
                return validationResult;

            var photoDto = new PhotoAddDTO
            {
                ContentType = photoUpload.ContentType,
                Folder = "conversations/" + conversationId,
                FileName = photoUpload.FileName,
                FileStream = photoUpload.FileStream,
                IsProfilePicture = false,
                UserId = sender.Id.ToString()
            };

            // 2. Generate unique filename and upload to storage
            var newUrl = await AddPhoto(photoDto, cancellationToken);

            // Set the URL in the photo data (we'll handle this in AddPhotoMessage)
            var photoData = new Dictionary<string, object>
            {
                ["fileName"] = photoUpload.FileName,
                ["url"] = newUrl,
                ["contentType"] = photoUpload.ContentType,
                ["sizeInBytes"] = photoUpload.FileStream.Length,
                ["uploadedAt"] = DateTime.UtcNow,
                ["caption"] = caption ?? photoUpload.Caption ?? ""
            };

            var conversationItemAdd = new FirestoreConversationItemAddDTO
            {
                Type = "photo",
                Data = photoData
            };

            // 4. Use the conversation service to add the item directly
            var conversation = await firestoreRepository.GetAsync<FirestoreConversationDTO>(
                "conversations", conversationId.ToString(), cancellationToken);
            var addResult = await conversationService.AddConversationItem(
                conversationItemAdd,
                Guid.Parse(conversationId.ToString()),
                sender,
                cancellationToken);

            if (!addResult.IsOk)
            {
                // If adding to conversation failed, clean up the uploaded image
                try
                {
                    var objectName = new Uri(newUrl).AbsolutePath.TrimStart('/');
                    await firebaseStorageService.DeleteImageAsync(objectName, cancellationToken);
                }
                catch (Exception cleanupEx)
                {
                    // Log cleanup failure but don't throw
                    // Logger should be injected for proper logging
                }
                
                return addResult;
            }

            return ServiceResponse.CreateSuccessResponse();
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse(
                new ErrorMessage(HttpStatusCode.InternalServerError, $"Failed to add photo: {ex.Message}"));
        }
    }
}