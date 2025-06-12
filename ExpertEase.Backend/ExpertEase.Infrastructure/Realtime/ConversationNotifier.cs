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
    
    public Task NotifyNewReply(Guid receiverUserId, object payload)
    {
        return hubContext.Clients.Group(receiverUserId.ToString())
            .SendAsync("ReceiveNewReply", payload);
    }
}
