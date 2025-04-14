using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcessService.Infrastructure.Broker;
using ProcessService.APP.Services;
using DotNetEnv;

Env.Load();

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IBrokerConnection, BrokerConnection>();
        services.AddSingleton<S3Service>();
        services.AddSingleton<VideoProcessor>();
        services.AddHostedService<VideoProcessorService>();
    })
    .Build();

await host.RunAsync();