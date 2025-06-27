namespace ExpertEase.Application.DataTransferObjects.StripeAccountDTOs;

public class StripeAccountStatusDTO
{
    public string AccountId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool ChargesEnabled { get; set; }
    public bool PayoutsEnabled { get; set; }
    public bool DetailsSubmitted { get; set; }
    public List<string> RequirementsCurrentlyDue { get; set; } = new();
    public List<string> RequirementsEventuallyDue { get; set; } = new();
    public string? DisabledReason { get; set; }
}