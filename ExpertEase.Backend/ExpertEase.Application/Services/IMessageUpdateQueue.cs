namespace ExpertEase.Application.Services;

public interface IMessageUpdateQueue
{
    void Enqueue(string messageId);
}