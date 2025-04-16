namespace ProcessService.APP.Interfaces
{
    public interface IS3Service
    {
        Task<byte[]> DownloadVideoAsync(string videoName);
        Task UploadZipAsync(string zipPath);
        Task DeleteOriginalVideoAsync(string videoName);
        string GetPresignedUrl(string objectKey, double durationInMinutes = 60);
    }
}
