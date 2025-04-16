using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using Amazon;
using ProcessService.Infrastructure.Services;
using System.Text;

namespace ProcessService.APP.Tests
{
    public class S3ServiceTests
    {
        private Mock<IAmazonS3> _mockS3Client;
        private S3Service _s3Service;

        [SetUp]
        public void Setup()
        {
            _mockS3Client = new Mock<IAmazonS3>(MockBehavior.Strict);
            _mockS3Client.Setup(x => x.Dispose()); // Allow Dispose in all tests
            _s3Service = new S3Service(_mockS3Client.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _s3Service.Dispose();
        }

        [Test]
        public void GetPresignedUrl_ShouldReturnUrl_WhenCalled()
        {
            var mockUrl = "https://bucket.s3.amazonaws.com/object";
            _mockS3Client.Setup(x => x.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>())).Returns(mockUrl);

            var url = _s3Service.GetPresignedUrl("some-object-key");

            Assert.That(url, Is.EqualTo(mockUrl));
        }

        [Test]
        public void GetPresignedUrl_ShouldThrow_WhenObjectKeyIsNullOrEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => _s3Service.GetPresignedUrl(null));
            Assert.Throws<ArgumentNullException>(() => _s3Service.GetPresignedUrl(""));
        }

        [Test]
        public async Task DownloadVideoAsync_ShouldReturnBytes_WhenObjectExists()
        {
            var videoName = "test.mp4";
            var expectedBytes = Encoding.UTF8.GetBytes("video-content");
            var memoryStream = new MemoryStream(expectedBytes);
            var mockResponse = new GetObjectResponse
            {
                ResponseStream = memoryStream
            };
            _mockS3Client.Setup(x => x.GetObjectAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                default
            )).ReturnsAsync(mockResponse);

            var result = await _s3Service.DownloadVideoAsync(videoName);

            Assert.That(result, Is.EqualTo(expectedBytes));
        }

        [Test]
        public void DownloadVideoAsync_ShouldThrow_WhenS3Throws()
        {
            _mockS3Client.Setup(x => x.GetObjectAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                default
            )).ThrowsAsync(new Exception("S3 error"));

            Assert.ThrowsAsync<Exception>(async () => await _s3Service.DownloadVideoAsync("video.mp4"));
        }

        [Test]
        public async Task UploadZipAsync_ShouldCallPutObjectAsync()
        {
            var zipPath = "test.zip";
            _mockS3Client.Setup(x => x.PutObjectAsync(
                It.IsAny<PutObjectRequest>(),
                default
            )).ReturnsAsync(new PutObjectResponse()).Verifiable();

            await _s3Service.UploadZipAsync(zipPath);

            _mockS3Client.Verify(x => x.PutObjectAsync(
                It.Is<PutObjectRequest>(r => r.FilePath == zipPath),
                default
            ), Times.Once);
        }

        [Test]
        public void UploadZipAsync_ShouldThrow_WhenS3Throws()
        {
            _mockS3Client.Setup(x => x.PutObjectAsync(
                It.IsAny<PutObjectRequest>(),
                default
            )).ThrowsAsync(new Exception("S3 error"));

            Assert.ThrowsAsync<Exception>(async () => await _s3Service.UploadZipAsync("file.zip"));
        }

        [Test]
        public async Task DeleteOriginalVideoAsync_ShouldCallDeleteObjectAsync()
        {
            _mockS3Client.Setup(x => x.DeleteObjectAsync(
                It.IsAny<DeleteObjectRequest>(),
                default
            )).ReturnsAsync(new DeleteObjectResponse()).Verifiable();

            await _s3Service.DeleteOriginalVideoAsync("video.mp4");

            _mockS3Client.Verify(x => x.DeleteObjectAsync(
                It.Is<DeleteObjectRequest>(r => r.Key.Contains("video.mp4")),
                default
            ), Times.Once);
        }

        [Test]
        public void DeleteOriginalVideoAsync_ShouldThrow_WhenS3Throws()
        {
            _mockS3Client.Setup(x => x.DeleteObjectAsync(
                It.IsAny<DeleteObjectRequest>(),
                default
            )).ThrowsAsync(new Exception("S3 error"));

            Assert.ThrowsAsync<Exception>(async () => await _s3Service.DeleteOriginalVideoAsync("video.mp4"));
        }

        [Test]
        public void Dispose_ShouldDisposeClient()
        {
            _mockS3Client.Setup(x => x.Dispose()).Verifiable();
            _s3Service.Dispose();
            _mockS3Client.Verify(x => x.Dispose(), Times.Once);
        }
    }
}
