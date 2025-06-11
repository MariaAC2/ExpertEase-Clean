using ExpertEase.Domain.Entities;
using Google.Cloud.Firestore;

namespace ExpertEase.Infrastructure.Firebase.FirestoreMappers;

public class MessageMapper
{
    public static FirestoreMessageDTO ToFirestoreDTO(Message message)
    {
        return new FirestoreMessageDTO
        {
            Id = message.Id.ToString(),
            SenderId = message.SenderId.ToString(),
            ConversationId = message.ConversationId.ToString(),
            Content = message.Content,
            IsRead = message.IsRead,
            CreatedAt = Timestamp.FromDateTime(message.CreatedAt.ToUniversalTime()),
            Attachment = message.Attachment != null ? new FirestorePhotoDTO
            {
                Url = message.Attachment.Url,
                FileName = message.Attachment.FileName,
                SizeInBytes = message.Attachment.SizeInBytes,
                ContentType = message.Attachment.ContentType
            } : null
        };
    }

    public static Message FromFirestoreDTO(FirestoreMessageDTO dto)
    {
        return new Message
        {
            Id = Guid.Parse(dto.Id),
            SenderId = Guid.Parse(dto.SenderId),
            ConversationId = Guid.Parse(dto.ConversationId),
            Content = dto.Content,
            IsRead = dto.IsRead,
            CreatedAt = dto.CreatedAt.ToDateTime(),
            Attachment = dto.Attachment != null ? new Photo
            {
                Url = dto.Attachment.Url,
                FileName = dto.Attachment.FileName,
                SizeInBytes = dto.Attachment.SizeInBytes,
                ContentType = dto.Attachment.ContentType
            } : null
        };
    }
}