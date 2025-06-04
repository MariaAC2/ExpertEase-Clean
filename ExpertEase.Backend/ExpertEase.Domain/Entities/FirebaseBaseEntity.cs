using Google.Cloud.Firestore;

namespace ExpertEase.Domain.Entities;

[FirestoreData]
public abstract class FirebaseBaseEntity
{
    [FirestoreDocumentId] 
    public string Id { get; set; } = null!;

    [FirestoreProperty]
    public DateTime CreatedAt { get; set; }

    [FirestoreProperty]
    public DateTime? UpdatedAt { get; set; }
}