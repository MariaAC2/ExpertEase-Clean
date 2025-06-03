using Google.Cloud.Firestore;

namespace ExpertEase.Domain.Entities;

[FirestoreData]
public abstract class FirebaseBaseEntity
{
    [FirestoreProperty]
    public Guid Id { get; set; }

    [FirestoreProperty]
    public DateTime CreatedAt { get; set; }

    [FirestoreProperty]
    public DateTime? UpdatedAt { get; set; }
}