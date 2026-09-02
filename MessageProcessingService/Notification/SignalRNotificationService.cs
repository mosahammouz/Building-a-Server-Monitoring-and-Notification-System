using MessageProcessingService.Configuration;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace MessageProcessingService.Notification;

public class SignalRNotificationService : INotificationService
{
    private readonly SignalRConfig _config;
    private HubConnection? _connection;

    public SignalRNotificationService(
        IOptions<SignalRConfig> options)
    {
        _config = options.Value;
    }

    private async Task EnsureConnectedAsync()
    {
        if (_connection == null)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_config.SignalRUrl)
                .WithAutomaticReconnect()
                .Build();
        }

        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
        }
    }

    public async Task SendAnomalyAlertAsync(string message)
    {
        await EnsureConnectedAsync();

        await _connection!.InvokeAsync(
            "SendAnomalyAlert",
            message);
    }

    public async Task SendHighUsageAlertAsync(string message)
    {
        await EnsureConnectedAsync();

        await _connection!.InvokeAsync(
            "SendHighUsageAlert",
            message);
    }
}