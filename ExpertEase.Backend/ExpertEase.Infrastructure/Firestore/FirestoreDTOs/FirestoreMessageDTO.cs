using ExpertEase.Domain.Entities;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Firestore.FirestoreDTOs;

[FirestoreData]
public class FirestoreMessageDTO : FirestoreBaseEntityDTO
{
    [FirestoreProperty]
    public string SenderId { get; set; } = null!;
    [FirestoreProperty]
    public string Content { get; set; } = null!;
    [FirestoreProperty]
    public bool IsRead { get; set; } = false;
    [FirestoreProperty]
    public string ConversationId { get; set; } = null!;
    [FirestoreProperty]
    public string Type { get; set; } = "message";
    [FirestoreProperty]
    public FirestorePhotoDTO? Attachment { get; set; } = null!;
}