using FFMpegCore;
using ProcessService.APP.Interfaces;
using System.Drawing;
using System.IO.Compression;

namespace ProcessService.Infrastructure.Services
{
    public class VideoProcessor : IVideoProcessor
    {
        public async Task<string> ProcessAsync(byte[] videoBytes)
        {
            var videoPath = Path.GetTempFileName();
            var outputDir = Path.Combine(Path.GetTempPath(), "frames_" + Guid.NewGuid());
            Directory.CreateDirectory(outputDir);

            await File.WriteAllBytesAsync(videoPath, videoBytes);

            try
            {
                var videoInfo = await FFProbe.AnalyseAsync(videoPath);
                var duration = videoInfo.Duration;
                var interval = TimeSpan.FromSeconds(20);

                for (var time = TimeSpan.Zero; time < duration; time += interval)
                {
                    var framePath = Path.Combine(outputDir, $"frame_{time.TotalSeconds}.jpg");
                    await FFMpeg.SnapshotAsync(videoPath, framePath, new Size(1920, 1080), time);
                }

                var zipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
                ZipFile.CreateFromDirectory(outputDir, zipPath);

                return zipPath;
            }
            finally
            {
                if (File.Exists(videoPath)) File.Delete(videoPath);
                if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
            }
        }
    }
}
