using System.Threading.Channels;
using ExpertEase.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExpertEase.Infrastructure.Workers;

public class MessageUpdateWorker(IServiceProvider serviceProvider, ILogger<MessageUpdateWorker> logger) : BackgroundService, IMessageUpdateQueue
{
    private readonly Channel<string> _queue = Channel.CreateUnbounded<string>();

    public void Enqueue(string messageId)
    {
        if (!_queue.Writer.TryWrite(messageId))
        {
            logger.LogWarning("Unable to enqueue message ID: {MessageId}", messageId);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var messageId in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

                await messageService.MarkMessageAsRead(messageId, stoppingToken);
                logger.LogInformation("Marked message {MessageId} as read", messageId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to mark message {MessageId} as read", messageId);
            }
        }
    }
}
