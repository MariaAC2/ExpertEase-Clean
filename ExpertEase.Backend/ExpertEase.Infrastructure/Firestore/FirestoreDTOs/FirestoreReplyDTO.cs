using Google.Cloud.Firestore;
using Org.BouncyCastle.Asn1.Cms;

namespace ExpertEase.Infrastructure.Firestore.FirestoreDTOs;

[FirestoreData]
public class FirestoreReplyDTO: FirestoreMessageDTO
{
    [FirestoreProperty]
    public Timestamp StartDate { get; set; }
    [FirestoreProperty]
    public Timestamp EndDate { get; set; }
    [FirestoreProperty]
    public decimal Price { get; set; }
    [FirestoreProperty]
    public string Status { get; set; } = "Pending";
}