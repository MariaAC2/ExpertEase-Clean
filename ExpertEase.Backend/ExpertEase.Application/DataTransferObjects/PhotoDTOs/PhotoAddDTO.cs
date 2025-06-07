namespace ExpertEase.Application.DataTransferObjects.PhotoDTOs;

public class PhotoAddDTO
{
    public Stream? FileStream { get; set; }
    public string Folder { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public bool IsProfilePicture { get; set; } = false;
}

public class ProfilePictureAddDTO
{
    public Stream FileStream { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

public class PortfolioPictureAddDTO
{
    public Stream FileStream { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
}