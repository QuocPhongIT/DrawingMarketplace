using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloadService _downloadService;

        public DownloadController(IDownloadService downloadService)
        {
            _downloadService = downloadService;
        }

        [SwaggerOperation(
            Summary = "Lấy danh sách file có thể tải của content",
            Description = "Trả về danh sách các file download theo contentId"
        )]
        [Authorize]
        [HttpGet("contents/{contentId}/downloads")]
        public async Task<IActionResult> GetDownloadFiles(Guid contentId)
        {
            var files = await _downloadService.GetDownloadFilesAsync(contentId);
            return Ok(files);
        }

        [SwaggerOperation(
            Summary = "Tải file",
            Description = "Tải file theo contentId và fileId"
        )]
        [Authorize]
        [HttpGet("{contentId}/downloads/{fileId}")]
        public async Task<IActionResult> DownloadFile(Guid contentId, Guid fileId)
        {
            var result = await _downloadService.DownloadFileAsync(contentId, fileId);

            return File(
                result.Stream,
                result.ContentType ?? "application/pdf",
                result.FileName
            );
        }
    }
}
