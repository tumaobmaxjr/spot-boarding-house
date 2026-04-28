namespace SpotBoardingHouse.Application.DTOs.Uploads
{
    public class MediaAssetUploadResultDto
    {
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
    }
}
