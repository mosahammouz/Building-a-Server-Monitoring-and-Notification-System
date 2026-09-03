using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ServerMonitoringService.Configuration;
using ServerMonitoringService.Models;

namespace ServerMonitoringService.Messaging;
public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly RabbitMqConfig _config;
    public RabbitMqMessagePublisher(IOptions<RabbitMqConfig> options)
    {
        _config = options.Value;
    }

    public async Task PublishAsync(
        ServerStatistics statistics,
        string topic,
        CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.HostName,
            Port = _config.Port,
            UserName = _config.UserName,
            Password = _config.Password
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);//connection => between worker and RabbitMq 

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken); // from chanel u can declare the exchange and queue

        await channel.ExchangeDeclareAsync( //create the exchange
            exchange: "ServerStatistics",
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        var message = JsonSerializer.Serialize(statistics);
        var body = Encoding.UTF8.GetBytes(message); // UTF-8 bytes message

        await channel.BasicPublishAsync( // send data through it
            exchange: "ServerStatistics",
            routingKey: topic,
            body: body,
            cancellationToken: cancellationToken);
    }
}