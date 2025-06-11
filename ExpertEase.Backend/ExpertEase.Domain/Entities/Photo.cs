namespace ExpertEase.Domain.Entities;

public class Photo
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeInBytes { get; set; }
    public bool IsProfilePicture { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}