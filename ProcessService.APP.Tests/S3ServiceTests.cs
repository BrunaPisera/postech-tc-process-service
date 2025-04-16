using Amazon.S3;
using Moq;
using Amazon;
using ProcessService.Infrastructure.Services;

namespace ProcessService.APP.Tests
{
    public class S3ServiceTests
    {
        private Mock<AmazonS3Client> _mockClient;
        private S3Service _s3Service;

        [SetUp]
        public void Setup()
        {
            // Simula variáveis de ambiente para AWS
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "accessKey");
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "secretKey");
            Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", "sessionToken");

            _mockClient = new Mock<AmazonS3Client>(MockBehavior.Strict, "accessKey", "secretKey", "sessionToken", RegionEndpoint.USEast1);
            _s3Service = new S3Service();
        }

        [TearDown]
        public void TearDown()
        {
            _s3Service.Dispose(); // Liberando recursos após cada teste
        }

        [Test]
        public void S3Service_ShouldThrow_WhenAWS_CredentialsAreMissing()
        {
            // Arrange - Simula variáveis de ambiente ausentes
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", string.Empty);
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", string.Empty);
            Environment.SetEnvironmentVariable("AWS_SESSION_TOKEN", string.Empty);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => new S3Service());
            Assert.That(ex.Message, Is.EqualTo("AWS credentials environment variables are missing."));
        }

        [Test]
        public void GetPresignedUrl_ShouldReturnUrl_WhenCalled()
        {
            // Act
            var url = _s3Service.GetPresignedUrl("some-object-key");

            // Assert
            Assert.That(url, Is.Not.Null.And.Not.Empty);
            Assert.That(Uri.IsWellFormedUriString(url, UriKind.Absolute), Is.True);
        }
    }
}
