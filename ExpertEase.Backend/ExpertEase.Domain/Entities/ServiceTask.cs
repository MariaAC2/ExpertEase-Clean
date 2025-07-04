﻿using ExpertEase.Domain.Enums;

namespace ExpertEase.Domain.Entities;

public class ServiceTask : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid SpecialistId { get; set; }
    public User Specialist { get; set; } = null!;
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; } = null!;
    public string Address { get; set; } = null!;
    public JobStatusEnum Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public decimal Price { get; set; }
    // 🆕 Add these fields for money transfer tracking
    public string? TransferReference { get; set; } // Stripe Transfer ID (e.g., "tr_1234567890")
    public DateTime? TransferredAt { get; set; }    // When the money was transferred
}