using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.UserDTOs;

namespace ExpertEase.Application.DataTransferObjects.SpecialistDTOs;

public class BecomeSpecialistResponseDTO
{
    [Required]
    public string Token { get; set; } = null!;
    [Required]
    public UserDTO User { get; set; } = null!;
}