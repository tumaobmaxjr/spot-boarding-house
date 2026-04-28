using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.DTOs.Uploads;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    public class UploadImageRequest
    {
        [FromForm(Name = "file")]
        public IFormFile? File { get; set; }
    }

    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Owner,Admin")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UploadsController : ControllerBase
    {
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "image/gif",
            "image/avif"
        };

        private readonly IMediaAssetStorageService _mediaAssetStorageService;
        private readonly IConfiguration _configuration;

        public UploadsController(
            IMediaAssetStorageService mediaAssetStorageService,
            IConfiguration configuration)
        {
            _mediaAssetStorageService = mediaAssetStorageService;
            _configuration = configuration;
        }

        [HttpPost("images")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)]
        public async Task<ActionResult<MediaAssetUploadResultDto>> UploadImage(
            [FromForm] UploadImageRequest request,
            CancellationToken cancellationToken)
        {
            var file = request.File;

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Image file is required." });
            }

            var maxSizeBytes = _configuration.GetValue<long?>("Uploads:MaxImageBytes") ?? (5 * 1024 * 1024);
            if (file.Length > maxSizeBytes)
            {
                return BadRequest(new { message = $"Image exceeds max size of {maxSizeBytes} bytes." });
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return BadRequest(new { message = "Only image files are allowed." });
            }

            try
            {
                await using var stream = file.OpenReadStream();

                var result = await _mediaAssetStorageService.SaveImageAsync(
                    stream,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    cancellationToken);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
