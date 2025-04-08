using Microsoft.Extensions.Hosting;
using Pedidos.Infrastructure.Broker;
using ProcessService.Infrastructure.Broker;
using System.Text.Json;

namespace ProcessService.APP.Services
{
    public class VideoProcessorService : BackgroundService
    {
        private readonly IBrokerConnection _brokerConnection;
        private readonly S3Service _s3Service;
        private readonly VideoProcessor _videoProcessor;
        private readonly string Exchange = "videoOperations";       

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
                routingKey: "video.uploaded",
                callback: async (videoName) => await ProcessVideoAsync(videoName));

            return Task.CompletedTask;
        }

        private async Task ProcessVideoAsync(string videoName)
        {
            try
            {
                Console.WriteLine($"Iniciando processamento: {videoName}");

                var videoUrl = await HandleVideoProcessingAsync(videoName);

                PublishVideoProcessedMessage(videoName, videoUrl);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erro ao processar vídeo {videoName}: {ex.Message}");
            }
        }

        private async Task<string> HandleVideoProcessingAsync(string videoName)
        {
            var videoBytes = await _s3Service.DownloadVideoAsync(videoName);
            var framesZipPath = await _videoProcessor.ProcessAsync(videoBytes);

            await _s3Service.UploadZipAsync(framesZipPath);
            await _s3Service.DeleteOriginalVideoAsync(videoName);

            File.Delete(framesZipPath);

            return _s3Service.GetPresignedUrl($"imagens/{Path.GetFileName(framesZipPath)}");
        }

        private void PublishVideoProcessedMessage(string videoName, string videoUrl)
        {
            var publisher = new BrokerPublisher(_brokerConnection);

            var message = new VideoProcessedMessage
            {
                FilesURL = videoUrl,
                VideoKey = videoName
            };

            publisher.PublishMessage(
                exchange: Exchange,
                message: JsonSerializer.Serialize(message),
                routingKey: "video.processed");
        }
    }
}
