using MessageProcessingService.Messaging;

namespace MessageProcessingService;

public class Worker : BackgroundService
{
    private readonly RabbitMqConsumer _consumer;

    public Worker(RabbitMqConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartAsync(stoppingToken);
    }
}