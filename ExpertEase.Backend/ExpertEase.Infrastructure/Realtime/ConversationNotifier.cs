using ExpertEase.Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace ExpertEase.Infrastructure.Realtime;

public class ConversationNotifier(IHubContext<ConversationHub> hubContext) : IConversationNotifier
{
    public Task NotifyNewMessage(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveNewMessage", payload);
    }

    public Task NotifyNewRequest(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveNewRequest", payload);
    }
    
    public Task NotifyRequestAccepted(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveRequestAccepted", payload);
    }
    
    public Task NotifyRequestRejected(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveRequestRejected", payload);
    }
    
    public Task NotifyRequestCancelled(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveRequestCancelled", payload);
    }
    
    public Task NotifyRequestUpdated(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveRequestUpdated", payload);
    }
    
    public Task NotifyRequestCompleted(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveRequestCompleted", payload);
    }
    
    public Task NotifyNewReply(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveNewReply", payload);
    }

    public Task NotifyReplyAccepted(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveReplyAccepted", payload);
    }
    
    public Task NotifyReplyRejected(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveReplyRejected", payload);
    }
    
    public Task NotifyReplyCancelled(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveReplyCancelled", payload);
    }
    
    public Task NotifyReplyUpdated(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveReplyUpdated", payload);
    }

    // ✅ Just add these two methods to your existing service
    public Task NotifyPaymentConfirmed(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceivePaymentConfirmed", payload);
    }
    
    public Task NotifyPaymentFailed(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceivePaymentFailed", payload);
    }
}
