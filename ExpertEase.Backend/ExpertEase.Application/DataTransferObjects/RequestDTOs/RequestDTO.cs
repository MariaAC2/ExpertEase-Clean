﻿using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.ServiceTaskDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.DataTransferObjects.RequestDTOs;

public class RequestDTO
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public Guid SenderId { get; set; }
    [Required]
    public DateTime RequestedStartDate { get; set; }
    [Required]
    public string Description { get; set; } = null!;

    public string SenderPhoneNumber { get; set; } = null;
    
    public string SenderAddress { get; set; } = null!;
    public StatusEnum Status { get; set; } = StatusEnum.Pending;
}