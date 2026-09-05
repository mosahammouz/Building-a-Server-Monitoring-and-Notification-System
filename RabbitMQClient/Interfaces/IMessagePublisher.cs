namespace RabbitMQClient.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(
        string message,
        string topic,
        CancellationToken cancellationToken = default);
}