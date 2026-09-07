using Microsoft.AspNetCore.SignalR;

namespace SignalRHubService.Hubs;

public class NotificationHub : Hub //the idea to send information to a client immediately
{
    public async Task SendAnomalyAlert(string message) //SignalR event 
    {
        await Clients.All.SendAsync("AnomalyAlert", message); // will be sent to all clients (3) using SignalR Event Consumer Service
    }

    public async Task SendHighUsageAlert(string message)
    {
        await Clients.All.SendAsync("HighUsageAlert", message);
    }
}