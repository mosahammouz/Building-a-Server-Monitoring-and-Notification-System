using ServerMonitoringService.Models;

namespace ServerMonitoringService.Messaging;

public interface IMessagePublisher //an abstraction for message queuing
{
    Task PublishAsync(
        ServerStatistics statistics,
        string topic,
        CancellationToken cancellationToken = default);
}