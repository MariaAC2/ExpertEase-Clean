using Google.Cloud.Firestore;

namespace ExpertEase.Domain.Entities;

[FirestoreData]
public class Message : FirebaseBaseEntity
{
    [FirestoreProperty]
    public Guid SenderId { get; set; }

    [FirestoreProperty]
    public Guid ReceiverId { get; set; }

    [FirestoreProperty]
    public string Content { get; set; } = null!;
    
    [FirestoreProperty]
    public bool IsRead { get; set; } = false;
}