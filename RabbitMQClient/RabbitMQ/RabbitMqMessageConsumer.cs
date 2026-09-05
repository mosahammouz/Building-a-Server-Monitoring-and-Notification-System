using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQClient.Configuration;
using RabbitMQClient.Interfaces;

namespace RabbitMQClient.RabbitMq;

public class RabbitMqMessageConsumer : IMessageConsumer
{
    private readonly RabbitMqConfig _config;

    public RabbitMqMessageConsumer(IOptions<RabbitMqConfig> options)
    {
        _config = options.Value;
    }

    public async Task ConsumeAsync(
        string queueName,
        string exchangeName,
        string routingKey,
        Func<string, Task> messageHandler,
        CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config.HostName,
            Port = _config.Port,
            UserName = _config.UserName,
            Password = _config.Password
        };

        var connection =
            await factory.CreateConnectionAsync(cancellationToken);

        var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

        var queue = await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: queue.QueueName,
            exchange: exchangeName,
            routingKey: routingKey,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            await messageHandler(message);
        };

        await channel.BasicConsumeAsync(
            queue: queue.QueueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: cancellationToken);

        Console.WriteLine("RabbitMQ consumer started.");

        await Task.Delay(
            Timeout.Infinite,
            cancellationToken);
    }
}