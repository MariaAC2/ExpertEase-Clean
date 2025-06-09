using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.AccountDTOs;

public class AccountDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Currency { get; set; } = null!;
    [Required]
    public decimal Balance { get; set; }
}

