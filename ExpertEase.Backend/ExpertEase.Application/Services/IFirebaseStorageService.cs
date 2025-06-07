namespace ExpertEase.Application.Services;

public interface IFirebaseStorageService
{
    public Task<string> UploadImageAsync(Stream stream, string folder, string fileName, string contentType);
    public Task DeleteImageAsync(string objectName, CancellationToken cancellationToken = default);
}