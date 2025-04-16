using Amazon.S3.Model;
using Amazon.S3;
using Amazon;
using ProcessService.APP.Interfaces;

namespace ProcessService.Infrastructure.Services
{
    public class S3Service : IS3Service, IDisposable
    {
        private readonly IAmazonS3 _client;
        private const string BucketName = "videouploadtc";

        // New constructor for dependency injection
        public S3Service(IAmazonS3 s3Client)
        {
            _client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        }

        public async Task<byte[]> DownloadVideoAsync(string videoName)
        {
            var response = await _client.GetObjectAsync(BucketName, $"videos/{videoName}");

            using var ms = new MemoryStream();
            await response.ResponseStream.CopyToAsync(ms);
            return ms.ToArray();
        }

        public async Task UploadZipAsync(string zipPath)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = $"imagens/{Path.GetFileName(zipPath)}",
                FilePath = zipPath
            };

            await _client.PutObjectAsync(putRequest);
        }

        public string GetPresignedUrl(string objectKey, double durationInMinutes = 60)
        {
            if (string.IsNullOrEmpty(objectKey))
                throw new ArgumentNullException(nameof(objectKey));
            var request = new GetPreSignedUrlRequest
            {
                BucketName = BucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddMinutes(durationInMinutes)
            };

            return _client.GetPreSignedURL(request);
        }

        public async Task DeleteOriginalVideoAsync(string videoName)
        {
            await _client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = BucketName,
                Key = $"videos/{videoName}"
            });
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
