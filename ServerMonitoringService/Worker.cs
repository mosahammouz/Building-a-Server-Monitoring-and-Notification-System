using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQClient.Interfaces;
using ServerMonitoringService.Configuration;
using ServerMonitoringService.Service;

namespace ServerMonitoringService;

public class Worker(
    ILogger<Worker> logger,
    StatisticsCollector statisticsCollector,
    IOptions<ServerStatisticsConfig> config,
    IMessagePublisher messagePublisher) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var statistics = statisticsCollector.Collect();

            var topic =
                $"ServerStatistics.{config.Value.ServerIdentifier}";

            var message =
                JsonSerializer.Serialize(statistics);

            await messagePublisher.PublishAsync(
                message,
                topic,
                stoppingToken);

            logger.LogInformation(
                "Statistics published to {Topic}",
                topic);

            await Task.Delay(
                TimeSpan.FromSeconds(
                    config.Value.SamplingIntervalSeconds),
                stoppingToken);
        }
    }
}