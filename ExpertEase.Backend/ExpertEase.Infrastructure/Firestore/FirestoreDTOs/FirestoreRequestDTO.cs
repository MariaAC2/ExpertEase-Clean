using ExpertEase.Infrastructure.Firestore.FirestoreDTOs;
using Google.Cloud.Firestore;

namespace ExpertEase.Domain.Entities;

[FirestoreData]
public class FirestoreRequestDTO: FirestoreMessageDTO
{
    [FirestoreProperty]
    public Timestamp RequestedStartDate { get; set; }
    [FirestoreProperty]
    public string PhoneNumber { get; set; } = null!;
    [FirestoreProperty]
    public string Address { get; set; } = null!;
    [FirestoreProperty]
    public string Description { get; set; } = null!;
    [FirestoreProperty]
    public string Status { get; set; } = "Pending";
}