using FFMpegCore.Exceptions;
using ProcessService.Infrastructure.Services;

namespace ProcessService.APP.Tests
{
    public class VideoProcessorTests
    {
        [Test]
        public void ProcessAsync_ShouldThrowFFMpegException_WhenInvalidVideoDataProvided()
        {
            // Arrange
            var processor = new VideoProcessor();
            var invalidBytes = new byte[] { 0x00, 0x01, 0x02 };

            // Act & Assert
            var ex = Assert.ThrowsAsync<FFMpegException>(async () =>
            {
                await processor.ProcessAsync(invalidBytes);
            });

            Assert.That(ex.Message, Does.Contain("ffprobe").IgnoreCase);
        }
    }
}