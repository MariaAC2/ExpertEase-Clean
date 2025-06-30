namespace ExpertEase.Application.Services;

public interface IConversationNotifier
{
    Task NotifyNewMessage(Guid receiverUserId, object payload);
    Task NotifyNewRequest(Guid receiverUserId, object payload);
    Task NotifyNewReply(Guid receiverUserId, object payload);
    Task NotifyRequestAccepted(Guid receiverUserId, object payload);
    public Task NotifyRequestRejected(Guid receiverUserId, object payload);
    public Task NotifyRequestCancelled(Guid receiverUserId, object payload);
    public Task NotifyRequestUpdated(Guid receiverUserId, object payload);
    public Task NotifyRequestCompleted(Guid receiverUserId, object payload);
    public Task NotifyReplyAccepted(Guid receiverUserId, object payload);
    public Task NotifyReplyRejected(Guid receiverUserId, object payload);
    public Task NotifyReplyCancelled(Guid receiverUserId, object payload);
    public Task NotifyReplyUpdated(Guid receiverUserId, object payload);
    public Task NotifyPaymentConfirmed(Guid paymentId, object payload);
    public Task NotifyPaymentFailed(Guid paymentId, object payload);
    Task NotifyServiceCompleted(Guid receiverUserId, object payload);
    Task NotifyReviewReceived(Guid receiverUserId, object payload);
    Task NotifyReviewPrompt(Guid receiverUserId, object payload);
    Task NotifyServiceStatusChanged(Guid receiverUserId, object payload);
}