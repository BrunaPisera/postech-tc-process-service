using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcessService.Infrastructure.Broker;
using DotNetEnv;
using ProcessService.APP.Interfaces;
using ProcessService.Infrastructure.Services;
using Amazon.S3;

Env.Load();

var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IAmazonS3, AmazonS3Client>(); 
        services.AddSingleton<IBrokerConnection, BrokerConnection>();
        services.AddSingleton<IS3Service, S3Service>();
        services.AddSingleton<IVideoProcessor, VideoProcessor>();
        services.AddHostedService<VideoProcessorService>();
    })
    .Build();

await host.RunAsync();