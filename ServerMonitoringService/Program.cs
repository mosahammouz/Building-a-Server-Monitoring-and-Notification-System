using ServerMonitoringService;
using ServerMonitoringService.Configuration;
using ServerMonitoringService.Service;
using RabbitMQClient.Interfaces;
using RabbitMQClient.RabbitMq;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ServerStatisticsConfig>(
    builder.Configuration.GetSection("ServerStatisticsConfig"));

builder.Services.Configure<RabbitMQClient.Configuration.RabbitMqConfig>(
    builder.Configuration.GetSection("RabbitMqConfig"));

builder.Services.AddSingleton<StatisticsCollector>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();

var host = builder.Build();
host.Run();