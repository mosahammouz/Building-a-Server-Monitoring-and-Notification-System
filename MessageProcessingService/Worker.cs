using System.Text.Json;
using MessageProcessingService.Models;
using MessageProcessingService.Service;
using RabbitMQClient.Interfaces;

namespace MessageProcessingService;

public class Worker : BackgroundService
{   private readonly SignalRAlertService _signalRAlertService;
    private readonly IMessageConsumer _consumer;
    private readonly AnomalyDetectionService _anomalyDetectionService;
    
    public Worker(IMessageConsumer consumer , AnomalyDetectionService anomalyDetectionService,SignalRAlertService signalRAlertService)
    {
        _consumer = consumer;
        _anomalyDetectionService = anomalyDetectionService;
        _signalRAlertService = signalRAlertService;

    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await _signalRAlertService.StartAsync(); // is vital
        
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
        var statistics = JsonSerializer.Deserialize<ServerStatistics>(message);
        if(statistics == null) return;
        var alerts = _anomalyDetectionService.Detect(statistics);
        foreach (var alert in alerts)
        {
            Console.WriteLine($"ALERT : {alert}");
            Console.WriteLine("***********");
            await _signalRAlertService.SendAnomalyAlertAsync(alert); //call the func from SignalR Alert Service (1)
        }

        await Task.CompletedTask;
    }
}