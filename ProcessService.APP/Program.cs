using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcessService.Infrastructure.Broker;
using DotNetEnv;
using ProcessService.APP.Interfaces;
using ProcessService.Infrastructure.Services;

Env.Load();

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IBrokerConnection, BrokerConnection>();
        services.AddSingleton<IS3Service, S3Service>();
        services.AddSingleton<IVideoProcessor, VideoProcessor>();
        services.AddHostedService<VideoProcessorService>();
    })
    .Build();

await host.RunAsync();