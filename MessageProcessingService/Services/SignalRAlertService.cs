using Microsoft.AspNetCore.SignalR.Client;

namespace MessageProcessingService.Service;

public class SignalRAlertService
{
    private readonly HubConnection _connection;

    public SignalRAlertService()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://signalr-hub:8080/notificationHub")
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task StartAsync()
    {
        await _connection.StartAsync();

        Console.WriteLine("Connected to SignalR Hub.");
    }

    public async Task SendAnomalyAlertAsync(string message)
    {
        await _connection.InvokeAsync(
            "SendAnomalyAlert",
            message);
    }

    public async Task SendHighUsageAlertAsync(string message)
    {
        await _connection.InvokeAsync(
            "SendHighUsageAlert",
            message);
    }
}