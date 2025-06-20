﻿using System.ComponentModel.DataAnnotations;
using ExpertEase.Application.DataTransferObjects.MessageDTOs;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.RequestDTOs;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Infrastructure.Firestore.FirestoreDTOs;

public class ConversationDTO
{
    public Guid ConversationId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public string UserFullName { get; set; } = null!;
    [Required]
    public string? UserProfilePictureUrl { get; set; } = null!;
    [Required]
    public List<FirestoreConversationItemDTO> ConversationItems { get; set; } = new();
}

public class UserConversationDTO
{
    [Required]
    public string UserId { get; set; }
    [Required]
    public string UserFullName { get; set; } = null!;
    [Required]
    public string? UserProfilePictureUrl { get; set; } = null!;
    public string LastMessage { get; set; } = null!;
    public DateTime LastMessageAt { get; set; } = DateTime.MinValue;
    public int UnreadCount { get; set; } = 0;
}

public class ConversationItemDTO
{
    public Guid Id { get; set; }
    public string ConversationId { get; set; } = null!;
    public string SenderId { get; set; } = null!;
    public string Type { get; set; } = null!; // "message", "request", "reply"
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}