using Microsoft.AspNetCore.SignalR;

namespace SignalRHubService.Hubs;

public class NotificationHub : Hub
{
    public async Task SendAnomalyAlert(string message)
    {
        await Clients.All.SendAsync("AnomalyAlert", message);
    }

    public async Task SendHighUsageAlert(string message)
    {
        await Clients.All.SendAsync("HighUsageAlert", message);
    }
}