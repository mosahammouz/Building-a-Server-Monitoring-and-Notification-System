namespace RabbitMQClient.Interfaces;

public interface IMessageConsumer
{
    Task ConsumeAsync(
        string queueName,
        string exchangeName,
        string routingKey,
        Func<string, Task> messageHandler,
        CancellationToken cancellationToken = default);
}