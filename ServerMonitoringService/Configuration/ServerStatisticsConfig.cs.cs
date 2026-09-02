namespace ServerMonitoringService.Configuration;

public class ServerStatisticsConfig
{
    public int SamplingIntervalSeconds { get; set; }
    public string ServerIdentifier { get; set; } = string.Empty;
}