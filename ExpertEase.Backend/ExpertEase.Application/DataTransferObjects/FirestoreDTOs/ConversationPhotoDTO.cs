namespace ExpertEase.Application.DataTransferObjects.FirestoreDTOs;

public class ConversationPhotoUploadDTO
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string? Caption { get; set; }
}