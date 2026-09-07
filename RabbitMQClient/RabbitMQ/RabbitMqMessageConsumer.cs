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
        string queueName,//server-statistics-queue
        string exchangeName,//ServerStatistics
        string routingKey,//ServerStatistics.* (linux1)
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

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        
        // Main Exchange
       await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

    
        // Dead Letter Exchange
        var deadLetterExchange = $"{exchangeName}.dlx";
        await channel.ExchangeDeclareAsync(
            exchange: deadLetterExchange,
            type: ExchangeType.Topic,
            durable: true,
            cancellationToken: cancellationToken);

      
        // Dead Letter Queue
        var deadLetterQueue = $"{queueName}.dlq";

        await channel.QueueDeclareAsync(
            queue: deadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync( // bindin dead letter queue
            queue: deadLetterQueue,
            exchange: deadLetterExchange,
            routingKey: routingKey,
            cancellationToken: cancellationToken);

    
        
        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = deadLetterExchange,
            ["x-dead-letter-routing-key"] = routingKey
        };
       // Main Queue
        var queue = await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: cancellationToken);

        // Bind Main Queue
        await channel.QueueBindAsync(
            queue: queue.QueueName,
            exchange: exchangeName,
            routingKey: routingKey,
            cancellationToken: cancellationToken);

        // Consumer
        var consumer = new AsyncEventingBasicConsumer(channel);// to listen to the messages in the queue

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                // Process the message
                await messageHandler(message); // functional programming

                // ACK only after successful processing
                await channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error processing message: {ex.Message}");

                // Reject the message and send it to the DLQ
                await channel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false);
            }
        };

       
        // Start Consuming
        await channel.BasicConsumeAsync(
            queue: queue.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        Console.WriteLine($"RabbitMQ consumer started. Queue: {queueName}");

        Console.WriteLine($"Dead Letter Queue: {deadLetterQueue}");

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}