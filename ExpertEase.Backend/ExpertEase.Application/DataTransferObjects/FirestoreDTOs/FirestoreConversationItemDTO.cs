using ExpertEase.Domain.Entities;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Firestore.FirestoreDTOs;

[FirestoreData]
public class FirestoreConversationItemDTO : FirestoreBaseEntityDTO
{
    [FirestoreProperty]
    public string ConversationId { get; set; } = null!;

    [FirestoreProperty]
    public string Type { get; set; } = null!; // "message", "request", "reply"

    [FirestoreProperty]
    public string SenderId { get; set; } = null!;

    [FirestoreProperty]
    public Timestamp CreatedAt { get; set; }

    [FirestoreProperty]
    public Dictionary<string, object> Data { get; set; } = new();
}
