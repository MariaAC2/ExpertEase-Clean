namespace ExpertEase.Application.DataTransferObjects.LoginDTOs;

public class SocialLoginDTO
{
    public string Provider { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}