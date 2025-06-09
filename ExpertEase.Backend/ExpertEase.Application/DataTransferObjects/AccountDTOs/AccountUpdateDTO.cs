using System.ComponentModel.DataAnnotations;

namespace ExpertEase.Application.DataTransferObjects.AccountDTOs;

public class AccountUpdateDTO
{
    [Required]
    public Guid UserId { get; set; }
    public string? Currency { get; set; }
    public decimal? Amount { get; set; }
}