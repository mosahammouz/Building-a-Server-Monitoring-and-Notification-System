using RabbitMQClient.Interfaces;

namespace MessageProcessingService;

public class Worker : BackgroundService
{
    private readonly IMessageConsumer _consumer;

    public Worker(IMessageConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await _consumer.ConsumeAsync(
            queueName: "server-statistics-queue",
            exchangeName: "ServerStatistics",
            routingKey: "ServerStatistics.*",
            messageHandler: HandleMessageAsync,
            cancellationToken: stoppingToken);
    }

    private async Task HandleMessageAsync(string message)
    {
        Console.WriteLine($"Received message: {message}");

        await Task.CompletedTask;
    }
}