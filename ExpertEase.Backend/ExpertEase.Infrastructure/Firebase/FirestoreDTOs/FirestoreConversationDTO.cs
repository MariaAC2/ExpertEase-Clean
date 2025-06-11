using Google.Cloud.Firestore;

namespace ExpertEase.Domain.Entities;

[FirestoreData]
public class FirestoreConversationDTO: FirestoreBaseEntityDTO
{
    [FirestoreProperty]
    public List<string> ParticipantIds { get; set; } = [];
    [FirestoreProperty]
    public string? LastMessage { get; set; }
    [FirestoreProperty]
    public Timestamp? LastMessageAt { get; set; }
    [FirestoreProperty]
    public Dictionary<string, int>? UnreadCounts { get; set; }
    [FirestoreProperty]
    public string RequestId { get; set; }
}