using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using SignalREventConsumerService.Configuration;

namespace SignalREventConsumerService.Messaging;

public class SignalREventConsumer
{
    private readonly SignalRConfig _config;

    public SignalREventConsumer(IOptions<SignalRConfig> options)
    {
        _config = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(_config.SignalRUrl)
            .WithAutomaticReconnect()
            .Build();

        connection.On<string>("AnomalyAlert", message =>
        {
            Console.WriteLine($"ANOMALY ALERT: {message}");
        });

        connection.On<string>("HighUsageAlert", message =>
        {
            Console.WriteLine($"HIGH USAGE ALERT: {message}");
        });

        await connection.StartAsync(cancellationToken);

        Console.WriteLine("SignalR event consumer started.");

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}