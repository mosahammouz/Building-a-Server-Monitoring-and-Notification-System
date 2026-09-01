using ServerMonitoringService;
using ServerMonitoringService.Configuration;
using ServerMonitoringService.Messaging;
using ServerMonitoringService.Service;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<ServerStatisticsConfig>(
    builder.Configuration.GetSection("ServerStatisticsConfig")); // u covert the json into c# class so u can access the SamplingIntervalSeconds and ServerIdentifier
builder.Services.AddSingleton<StatisticsCollector>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IMessagePublisher, ConsoleMessagePublisher>();

var host = builder.Build();
host.Run();