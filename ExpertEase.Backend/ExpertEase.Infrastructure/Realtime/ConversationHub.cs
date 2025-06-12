using Microsoft.AspNetCore.SignalR;

namespace ExpertEase.Infrastructure.Realtime;

public class ConversationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("Client connected: " + Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public async Task Join(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        Console.WriteLine($"User {userId} joined group {userId}");
    }
}