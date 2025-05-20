﻿using System.ComponentModel.DataAnnotations;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects;

public class StatusUpdateDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public StatusEnum Status { get; set; }
}

public class JobStatusUpdateDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public JobStatusEnum Status { get; set; }
}