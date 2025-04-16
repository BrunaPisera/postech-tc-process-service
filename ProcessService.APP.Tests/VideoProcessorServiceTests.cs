using Moq;
using ProcessService.APP.Interfaces;
using ProcessService.Infrastructure.Broker;
using ProcessService.Infrastructure.Services;
using RabbitMQ.Client;
using System.Text;

namespace ProcessService.APP.Tests
{
    public class VideoProcessorServiceTests
    {
        private Mock<IBrokerConnection> _mockBrokerConnection;
        private Mock<IS3Service> _mockS3Service;
        private Mock<IVideoProcessor> _mockVideoProcessor;
        private Mock<IModel> _model;
        private VideoProcessorService _service;


        [SetUp]
        public void SetUp()
        {
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "fake");
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "fake");
            Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", "fake");

            _mockBrokerConnection = new Mock<IBrokerConnection>();
            _mockS3Service = new Mock<IS3Service>();
            _mockVideoProcessor = new Mock<IVideoProcessor>();
            _model = new Mock<IModel>();

            _mockS3Service.Setup(x => x.DownloadVideoAsync(It.IsAny<string>()))
                .ReturnsAsync(new byte[] { 0x01, 0x02 });

            _mockS3Service.Setup(x => x.UploadZipAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockS3Service.Setup(x => x.DeleteOriginalVideoAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockS3Service.Setup(x => x.GetPresignedUrl(It.IsAny<string>(), It.IsAny<double>()))
                .Returns("https://fakeurl.com/fake.zip");

            _mockVideoProcessor.Setup(x => x.ProcessAsync(It.IsAny<byte[]>()))
                .ReturnsAsync("fakepath.zip");

            _mockBrokerConnection.Setup(x => x.CreateChannel()).Returns(_model.Object);

            _service = new VideoProcessorService(
                _mockBrokerConnection.Object,
                _mockS3Service.Object,
                _mockVideoProcessor.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _service.Dispose();
        }

        [Test]
        public async Task ProcessVideoAsync_ShouldProcessVideo_WhenCalled()
        {
            // Arrange
            var videoName = "test-video.mp4";
            var videoBytes = Encoding.UTF8.GetBytes("fake-video-content");
            var fakeZipPath = Path.GetTempFileName();
            var fakeUrl = "https://example.com/fake.zip";

            _mockS3Service.Setup(x => x.DownloadVideoAsync(videoName))
                          .ReturnsAsync(videoBytes);
            _mockVideoProcessor.Setup(x => x.ProcessAsync(videoBytes))
                               .ReturnsAsync(fakeZipPath);
            _mockS3Service.Setup(x => x.UploadZipAsync(fakeZipPath))
                          .Returns(Task.CompletedTask);
            _mockS3Service.Setup(x => x.DeleteOriginalVideoAsync(videoName))
                          .Returns(Task.CompletedTask);
            _mockS3Service.Setup(x => x.GetPresignedUrl(It.IsAny<string>(), It.IsAny<double>()))
                          .Returns(fakeUrl);

            // Cria arquivo fake zip para simular File.Delete
            File.WriteAllText(fakeZipPath, "fake content");

            // Act
            var method = typeof(VideoProcessorService)
                         .GetMethod("ProcessVideoAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            await (Task)method.Invoke(_service, new object[] { videoName });

            // Assert
            _mockS3Service.Verify(x => x.DownloadVideoAsync(videoName), Times.Once);
            _mockVideoProcessor.Verify(x => x.ProcessAsync(videoBytes), Times.Once);
            _mockS3Service.Verify(x => x.UploadZipAsync(fakeZipPath), Times.Once);
            _mockS3Service.Verify(x => x.DeleteOriginalVideoAsync(videoName), Times.Once);
        }
    }
}
