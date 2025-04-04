using Microsoft.Extensions.Hosting;
using ProcessService.Infrastructure.Broker;

namespace ProcessService.APP.Services
{
    public class VideoProcessorService : BackgroundService
    {
        private readonly IBrokerConnection _brokerConnection;
        private readonly S3Service _s3Service;
        private readonly VideoProcessor _videoProcessor;

        public VideoProcessorService(IBrokerConnection brokerConnection, S3Service s3Service, VideoProcessor videoProcessor)
        {
            _brokerConnection = brokerConnection;
            _s3Service = s3Service;
            _videoProcessor = videoProcessor;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new BrokerConsumer(_brokerConnection);

            consumer.BrokerStartConsumer<string>(
                queueName: "videoToProcess",
                exchange: "videoOperations",
                callback: async (videoName) =>
                {
                    Console.WriteLine($"Iniciando processamento: {videoName}");

                    var videoBytes = await _s3Service.DownloadVideoAsync(videoName);
                    var framesZipPath = await _videoProcessor.ProcessAsync(videoBytes);
                    await _s3Service.UploadZipAsync(framesZipPath);
                    await _s3Service.DeleteOriginalVideoAsync(videoName);

                    File.Delete(framesZipPath);
                });

            return Task.CompletedTask;
        }
    }
}
