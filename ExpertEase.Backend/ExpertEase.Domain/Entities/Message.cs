using Google.Cloud.Firestore;

namespace ExpertEase.Domain.Entities;

// using Google.Cloud.Firestore;
//
// [FirestoreData]
// public class Message
// {
//     [FirestoreDocumentId]
//     public string? Id { get; set; }
//
//     [FirestoreProperty]
//     public string SenderId { get; set; } = null!;
//
//     [FirestoreProperty]
//     public string ReceiverId { get; set; } = null!;
//
//     [FirestoreProperty]
//     public string Content { get; set; } = null!;
//
//     [FirestoreProperty]
//     public Timestamp Timestamp { get; set; }
//
//     [FirestoreProperty]
//     public string ConversationId { get; set; } = null!;
//
//     [FirestoreProperty]
//     public bool IsRead { get; set; }
//
//     [FirestoreProperty]
//     public List<Attachment>? Attachments { get; set; }
// }
//
// [FirestoreData]
// public class Attachment
// {
//     [FirestoreProperty]
//     public string Type { get; set; } = null!;
//
//     [FirestoreProperty]
//     public string Url { get; set; } = null!;
// }

[FirestoreData]
public class Message : FirebaseBaseEntity
{
    [FirestoreProperty] 
    public string SenderId { get; set; } = null!;

    [FirestoreProperty]
    public string ReceiverId { get; set; } = null!;

    [FirestoreProperty]
    public string Content { get; set; } = null!;
    
    [FirestoreProperty]
    public bool IsRead { get; set; } = false;
}