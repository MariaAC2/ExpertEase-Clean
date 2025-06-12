namespace ExpertEase.Application.Services;

public interface IConversationNotifier
{
    Task NotifyNewMessage(Guid receiverUserId, object payload);
    Task NotifyNewRequest(Guid receiverUserId, object payload);
    Task NotifyNewReply(Guid receiverUserId, object payload);
}