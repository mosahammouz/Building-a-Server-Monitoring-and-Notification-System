using MessageProcessingService.Configuration;
using MessageProcessingService.Models;
using Microsoft.Extensions.Options;

namespace MessageProcessingService.Service;

public class AnomalyDetectionService
{
    private readonly AnomalyDetectionConfig _config;

    // Stores the previous statistics for each server.
    private readonly Dictionary<string, ServerStatistics> _previousStatistics = new();

    public AnomalyDetectionService(IOptions<AnomalyDetectionConfig> options)
    {
        _config = options.Value;
    }

    public List<string> Detect(ServerStatistics current)
    {
        var alerts = new List<string>();

        _previousStatistics.TryGetValue(
            current.ServerIdentifier,
            out var previous);

        // 1. Memory usage anomaly
        if (previous != null &&
            current.MemoryUsage >
            previous.MemoryUsage *
            (1 + _config.MemoryUsageAnomalyThresholdPercentage))
        {
            alerts.Add(
                $"Memory usage anomaly detected on {current.ServerIdentifier}. " +
                $"Previous: {previous.MemoryUsage:F2} MB, " +
                $"Current: {current.MemoryUsage:F2} MB.");
        }

        // 2. CPU usage anomaly
        if (previous != null &&
            current.CpuUsage >
            previous.CpuUsage *
            (1 + _config.CpuUsageAnomalyThresholdPercentage))
        {
            alerts.Add(
                $"CPU usage anomaly detected on {current.ServerIdentifier}. " +
                $"Previous: {previous.CpuUsage:F2}%, " +
                $"Current: {current.CpuUsage:F2}%.");
        }

        // 3. High memory usage
        var totalMemory =
            current.MemoryUsage + current.AvailableMemory;

        if (totalMemory > 0)
        {
            var memoryUsageRatio =
                current.MemoryUsage / totalMemory;

            if (memoryUsageRatio >
                _config.MemoryUsageThresholdPercentage)
            {
                alerts.Add(
                    $"High memory usage detected on {current.ServerIdentifier}. " +
                    $"Usage: {memoryUsageRatio:P2}.");
            }
        }

        // 4. High CPU usage
        if (current.CpuUsage >
            _config.CpuUsageThresholdPercentage)
        {
            alerts.Add(
                $"High CPU usage detected on {current.ServerIdentifier}. " +
                $"Usage: {current.CpuUsage:F2}%.");
        }

        // Store current statistics as previous statistics
        // for the next message from this server.
        _previousStatistics[current.ServerIdentifier] = current;

        return alerts;
    }
}