﻿namespace ExpertEase.Application.DataTransferObjects.ProtectionFeeDTOs;

public class ProtectionFeeConfigurationDTO
{
    public string FeeType { get; set; } = "percentage"; // "percentage", "fixed", "hybrid"
    public decimal PercentageRate { get; set; }
    public decimal FixedAmount { get; set; }
    public decimal MinimumFee { get; set; }
    public decimal MaximumFee { get; set; }
    public bool IsEnabled { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class ProtectionFeeBreakdownDTO
{
    public decimal BaseServiceAmount { get; set; }
    public string FeeType { get; set; } = string.Empty;
    public decimal PercentageRate { get; set; }
    public decimal FixedAmount { get; set; }
    public decimal MinimumFee { get; set; }
    public decimal MaximumFee { get; set; }
    public decimal CalculatedFeeBeforeLimits { get; set; }
    public decimal FinalFee { get; set; }
    public string Justification { get; set; } = string.Empty;
    public bool MinimumApplied { get; set; }
    public bool MaximumApplied { get; set; }
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Enhanced response with detailed breakdown
/// </summary>
public class DetailedProtectionFeeResponseDTO
{
    public decimal ServiceAmount { get; set; }
    public decimal ProtectionFee { get; set; }
    public decimal TotalAmount { get; set; }
    public ProtectionFeeBreakdownDTO Breakdown { get; set; } = new();
    public ProtectionFeeConfigurationDTO Configuration { get; set; } = new();
    public string Summary { get; set; } = string.Empty; // User-friendly summary
}

public class CalculateProtectionFeeRequestDTO
{
    public decimal ServiceAmount { get; set; }
}

/// <summary>
/// Response DTO for protection fee calculation
/// </summary>
public class CalculateProtectionFeeResponseDTO
{
    public decimal ServiceAmount { get; set; }
    public decimal ProtectionFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string FeeJustification { get; set; } = string.Empty;
    public ProtectionFeeConfigurationDTO FeeConfiguration { get; set; } = new();
}