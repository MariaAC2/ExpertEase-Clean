using Google.Cloud.Firestore;

namespace ExpertEase.Domain.Entities;

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
    public FirestorePhotoDTO? Attachment { get; set; } = null!;
}