using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.AccountDTOs;

public class AccountAddDTO
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string Currency { get; set; } = null!;
    public decimal InitialBalance { get; set; } = 0;
}