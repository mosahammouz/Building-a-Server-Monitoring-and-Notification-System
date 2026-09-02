using System.Text.Json;
using ServerMonitoringService.Models;

namespace ServerMonitoringService.Messaging;

public class ConsoleMessagePublisher : IMessagePublisher
{
    public Task PublishAsync(
        ServerStatistics statistics,
        string topic,
        CancellationToken cancellationToken = default)
    {
        var message = JsonSerializer.Serialize(statistics);
        Console.WriteLine("*************************************");
        Console.WriteLine($"Publishing to topic: {topic}");
        Console.WriteLine($"Message: {message}");

        return Task.CompletedTask;
    }
}