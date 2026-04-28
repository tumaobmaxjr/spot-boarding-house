using SpotBoardingHouse.Application.DTOs.Uploads;

namespace SpotBoardingHouse.Application.Interfaces.Services
{
    public interface IMediaAssetStorageService
    {
        Task<MediaAssetUploadResultDto> SaveImageAsync(
            Stream fileStream,
            string originalFileName,
            string contentType,
            long sizeBytes,
            CancellationToken cancellationToken = default);
    }
}
