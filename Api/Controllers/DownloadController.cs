using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [Authorize]
        [HttpGet("contents/{contentId}/download")]
        public async Task<IActionResult> Download(Guid contentId)
        {
            var files = await _downloadService.GetDownloadFilesAsync(contentId);
            return Ok(files);
        }
    }
}
