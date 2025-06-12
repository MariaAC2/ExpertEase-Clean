using ExpertEase.Application.DataTransferObjects.UserDTOs;
using Google.Cloud.Firestore;

namespace ExpertEase.Domain.Entities;

[FirestoreData]
public class FirestoreConversationDTO: FirestoreBaseEntityDTO
{
    [FirestoreProperty]
    public List<string> ParticipantIds { get; set; } = [];
    [FirestoreProperty]
    public string Participants { get; set; } = null!;
    [FirestoreProperty]
    public string? LastMessage { get; set; }
    [FirestoreProperty]
    public Timestamp? LastMessageAt { get; set; }
    [FirestoreProperty]
    public Dictionary<string, int>? UnreadCounts { get; set; }
    [FirestoreProperty]
    public FirestoreUserConversationDTO ClientData { get; set; } = new FirestoreUserConversationDTO();
    [FirestoreProperty]
    public FirestoreUserConversationDTO SpecialistData { get; set; } = new FirestoreUserConversationDTO();
    [FirestoreProperty] 
    public string RequestId { get; set; } = null!;
}

[FirestoreData]
public class FirestoreUserConversationDTO
{
    [FirestoreProperty]
    public string UserId { get; set; }

    [FirestoreProperty]
    public string UserFullName { get; set; } = null!;

    [FirestoreProperty]
    public string? UserProfilePictureUrl { get; set; }
}