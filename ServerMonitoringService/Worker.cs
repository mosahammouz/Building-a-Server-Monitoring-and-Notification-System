using Microsoft.Extensions.Options;
using ServerMonitoringService.Configuration;
using ServerMonitoringService.Messaging;
using ServerMonitoringService.Service;

namespace ServerMonitoringService;

public class Worker(
    ILogger<Worker> logger,
    StatisticsCollector statisticsCollector,
    IOptions<ServerStatisticsConfig> config,
    IMessagePublisher messagePublisher) : BackgroundService // messagePublisher is a RabbitMqMessagePublisher in the DI container
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken) // when u press ctrl+c stoppingToken will be true
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var statistics = statisticsCollector.Collect(); // free -m  command will be executed 
            var topic = $"ServerStatistics.{config.Value.ServerIdentifier}";

            await messagePublisher.PublishAsync(
                statistics,
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