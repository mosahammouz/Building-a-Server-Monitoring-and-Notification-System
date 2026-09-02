using System.Text;
using System.Text.Json;
using MessageProcessingService.Configuration;
using MessageProcessingService.Data;
using MessageProcessingService.Models;
using MessageProcessingService.Notification;
using MessageProcessingService.Service;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageProcessingService.Messaging;

public class RabbitMqConsumer
{
    private readonly MongoDbContext _mongoDbContext;
    private readonly RabbitMqConfig _config;
    private readonly AnomalyDetectionService _anomalyDetectionService;
    private readonly INotificationService _notificationService;

    public RabbitMqConsumer(
        IOptions<RabbitMqConfig> options,
        MongoDbContext mongoDbContext,
        AnomalyDetectionService anomalyDetectionService,
        INotificationService notificationService)
    {
        _config = options.Value;
        _mongoDbContext = mongoDbContext;
        _anomalyDetectionService = anomalyDetectionService;
        _notificationService = notificationService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.HostName,
            Port = _config.Port,
            UserName = _config.UserName,
            Password = _config.Password
        };

        var connection = await factory.CreateConnectionAsync(cancellationToken);

        var channel = await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: "ServerStatistics",
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        var queue = await channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: queue.QueueName,
            exchange: "ServerStatistics",
            routingKey: "ServerStatistics.*",
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var statistics =
                JsonSerializer.Deserialize<ServerStatistics>(message);

            if (statistics != null)
            {
                // Save statistics to MongoDB
                await _mongoDbContext.ServerStatistics
                    .InsertOneAsync(statistics);

                // Detect anomalies and high usage
                var alerts =
                    _anomalyDetectionService.Detect(statistics);

                foreach (var alert in alerts)
                {
                    Console.WriteLine($"ALERT: {alert}");

                    // Send the alert through SignalR
                    if (alert.Contains("anomaly detected"))
                    {
                        await _notificationService
                            .SendAnomalyAlertAsync(alert);
                    }
                    else if (alert.Contains("High"))
                    {
                        await _notificationService
                            .SendHighUsageAlertAsync(alert);
                    }
                }

                Console.WriteLine(
                    $"Server: {statistics.ServerIdentifier}, " +
                    $"Memory: {statistics.MemoryUsage} MB, " +
                    $"Available: {statistics.AvailableMemory} MB, " +
                    $"CPU: {statistics.CpuUsage}%, " +
                    $"Time: {statistics.Timestamp}");
            }

            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(
            queue: queue.QueueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: cancellationToken);

        Console.WriteLine("RabbitMQ consumer started.");

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}