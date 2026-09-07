using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalREventConsumerService.Configuration;
using SignalREventConsumerService.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SignalRConfig>(
    builder.Configuration.GetSection("SignalRConfig"));

builder.Services.AddSingleton<SignalREventConsumer>();

var host = builder.Build();

var consumer = host.Services.GetRequiredService<SignalREventConsumer>();

await consumer.StartAsync(CancellationToken.None);