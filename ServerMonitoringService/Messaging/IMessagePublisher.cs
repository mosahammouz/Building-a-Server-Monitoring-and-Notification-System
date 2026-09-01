using ServerMonitoringService.Models;

namespace ServerMonitoringService.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(
        ServerStatistics statistics,
        string topic,
        CancellationToken cancellationToken = default);
}