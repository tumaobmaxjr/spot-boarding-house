using Microsoft.Extensions.Configuration;
using SpotBoardingHouse.Application.DTOs.Uploads;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Infrastructure.Services
{
    public class LocalMediaAssetStorageService : IMediaAssetStorageService
    {
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",
            "image/avif"
        };

        private static readonly Dictionary<string, string> ContentTypeToExtension =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["image/jpeg"] = ".jpg",
                ["image/png"] = ".png",
                ["image/webp"] = ".webp",
                ["image/gif"] = ".gif",
                ["image/avif"] = ".avif"
            };

        private readonly string _rootPath;
        private readonly string _publicBasePath;

        public LocalMediaAssetStorageService(IConfiguration configuration)
        {
            _rootPath = configuration["Uploads:RootPath"] ?? "uploads";
            _publicBasePath = (configuration["Uploads:PublicBasePath"] ?? "/uploads").TrimEnd('/');
        }

        public async Task<MediaAssetUploadResultDto> SaveImageAsync(
            Stream fileStream,
            string originalFileName,
            string contentType,
            long sizeBytes,
            CancellationToken cancellationToken = default)
        {
            if (!AllowedContentTypes.Contains(contentType))
            {
                throw new InvalidOperationException("Unsupported image content type.");
            }

            var extension = ResolveExtension(originalFileName, contentType);
            var now = DateTime.UtcNow;

            var targetDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                _rootPath,
                "images",
                now.ToString("yyyy"),
                now.ToString("MM"));

            Directory.CreateDirectory(targetDirectory);

            var generatedFileName = $"{Guid.NewGuid():N}{extension}";
            var absolutePath = Path.Combine(targetDirectory, generatedFileName);

            await using (var outputStream = new FileStream(
                absolutePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true))
            {
                await fileStream.CopyToAsync(outputStream, cancellationToken);
            }

            var rootAbsolutePath = Path.Combine(Directory.GetCurrentDirectory(), _rootPath);
            var relativePath = Path.GetRelativePath(rootAbsolutePath, absolutePath).Replace("\\", "/");
            var url = $"{_publicBasePath}/{relativePath}";

            return new MediaAssetUploadResultDto
            {
                Url = url,
                FileName = generatedFileName,
                ContentType = contentType,
                SizeBytes = sizeBytes
            };
        }

        private static string ResolveExtension(string originalFileName, string contentType)
        {
            var fileNameExtension = Path.GetExtension(originalFileName);

            if (!string.IsNullOrWhiteSpace(fileNameExtension))
            {
                return fileNameExtension.ToLowerInvariant();
            }

            if (ContentTypeToExtension.TryGetValue(contentType, out var mappedExtension))
            {
                return mappedExtension;
            }

            throw new InvalidOperationException("Cannot determine file extension.");
        }
    }
}
