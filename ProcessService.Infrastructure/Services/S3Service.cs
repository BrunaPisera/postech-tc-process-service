using Amazon.S3.Model;
using Amazon.S3;
using Amazon;
using ProcessService.APP.Interfaces;

namespace ProcessService.Infrastructure.Services
{
    public class S3Service : IS3Service, IDisposable
    {
        private readonly AmazonS3Client _client;
        private const string BucketName = "videouploadtc";

        public S3Service()
        {
            var accessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var sessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");

            if (string.IsNullOrEmpty(accessKeyId) ||
                string.IsNullOrEmpty(secretAccessKey) ||
                string.IsNullOrEmpty(sessionToken))
            {
                throw new Exception("AWS credentials environment variables are missing.");
            }

            _client = new AmazonS3Client(accessKeyId, 
                                       secretAccessKey,
                                       sessionToken,
                                       RegionEndpoint.USEast1);
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
