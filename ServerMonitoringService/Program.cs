using ServerMonitoringService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();// Singleton lifetime

var host = builder.Build();
host.Run();