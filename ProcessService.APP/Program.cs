using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Drawing;
using System.IO.Compression;
using FFMpegCore;
using ProcessService.Infrastructure.Broker;

var rabbitMqConnection = new BrokerConnection();
var rabbitMqConsumer = new BrokerConsumer(rabbitMqConnection);

rabbitMqConsumer.BrokerStartConsumer<string>(
    queueName: "videoToProcess",
    exchange: "videoOperations",   
    callback: async (videoName) => {
        using (var client = new AmazonS3Client("", "",
                 "",
                 RegionEndpoint.USEast1))
        {
            GetObjectResponse response = await client.GetObjectAsync("videouploadtc", $"videos/{videoName}");

            MemoryStream memoryStream = new MemoryStream();

            using Stream responseStream = response.ResponseStream;

            responseStream.CopyTo(memoryStream);

            var video = memoryStream.ToArray();

            var videoPath = Path.GetTempFileName();
            Console.WriteLine($"Video path: {videoPath}");
            try
            {
                File.WriteAllBytes(videoPath, video);

                var outputFolder = Path.GetTempPath() + "imagens";

                Directory.CreateDirectory(outputFolder);

                var videoInfo = FFProbe.Analyse(videoPath);
                var duration = videoInfo.Duration;

                var interval = TimeSpan.FromSeconds(20);

                for (var currentTime = TimeSpan.Zero; currentTime < duration; currentTime += interval)
                {
                    Console.WriteLine($"Processando frame: {currentTime}");

                    var outputPath = Path.Combine(outputFolder, $"frame_at_{currentTime.TotalSeconds}.jpg");
                    FFMpeg.Snapshot(videoPath, outputPath, new Size(1920, 1080), currentTime);
                }

                string destinationZipFilePath = @$"{Path.GetTempPath()}{Guid.NewGuid()}.zip";

                ZipFile.CreateFromDirectory(outputFolder, destinationZipFilePath);

                var putRequest = new PutObjectRequest
                {
                    BucketName = "videouploadtc",
                    Key = $"imagens/{Path.GetFileName(destinationZipFilePath)}",
                    FilePath = destinationZipFilePath
                };

                var putResponse = await client.PutObjectAsync(putRequest);

                if (File.Exists(destinationZipFilePath)) File.Delete(destinationZipFilePath);
                if (Directory.Exists(outputFolder)) Directory.Delete(outputFolder, true);

                var deleteVideo = new DeleteObjectRequest
                {
                    BucketName = "videouploadtc",
                    Key = $"videos/{videoName}",
                };

                await client.DeleteObjectAsync(deleteVideo);
            }
            finally
            {
                if (File.Exists(videoPath)) File.Delete(videoPath);
            }
        }

    });

Console.ReadLine();