using MessageProcessingService;
using MessageProcessingService.Configuration;
using MessageProcessingService.Messaging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<RabbitMqConfig>(
    builder.Configuration.GetSection("RabbitMqConfig"));

builder.Services.Configure<MongoDbConfig>(
    builder.Configuration.GetSection("MongoDbConfig"));

builder.Services.AddSingleton<RabbitMqConsumer>();


var host = builder.Build();
host.Run();