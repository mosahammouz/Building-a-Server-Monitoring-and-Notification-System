using SignalRHubService.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<NotificationHub>("/notificationHub");//8080

app.Run();