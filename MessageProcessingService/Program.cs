using MessageProcessingService;
using MessageProcessingService.Configuration;
using MessageProcessingService.Data;
using MessageProcessingService.Notification;
using MessageProcessingService.Service;
using RabbitMQClient.Configuration;
using RabbitMQClient.Interfaces;
using RabbitMQClient.RabbitMq;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.Configure<RabbitMQClient.Configuration.RabbitMqConfig>(
    builder.Configuration.GetSection("RabbitMqConfig"));

builder.Services.Configure<MongoDbConfig>(
    builder.Configuration.GetSection("MongoDbConfig"));

builder.Services.Configure<AnomalyDetectionConfig>(
    builder.Configuration.GetSection("AnomalyDetectionConfig"));

builder.Services.Configure<SignalRConfig>(
    builder.Configuration.GetSection("SignalRConfig"));

builder.Services.AddSingleton<AnomalyDetectionService>();
builder.Services.AddSingleton<IMessageConsumer, RabbitMqMessageConsumer>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<INotificationService, SignalRNotificationService>();

var host = builder.Build();

host.Run();