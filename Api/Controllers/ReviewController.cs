using DrawingMarketplace.Application.DTOs.Reviews;
using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [Authorize]
        [HttpPost("contents/{contentId}")]
        public async Task<IActionResult> Create(Guid contentId, CreateReviewDto dto)
        {
            await _reviewService.CreateAsync(contentId, dto);
            return Ok();
        }

        [HttpGet("contents/{contentId}")]
        public async Task<IActionResult> GetByContent(Guid contentId)
        {
            return Ok(await _reviewService.GetByContentAsync(contentId));
        }

        [Authorize]
        [HttpPut("{reviewId}")]
        public async Task<IActionResult> Update(Guid reviewId, CreateReviewDto dto)
        {
            await _reviewService.UpdateAsync(reviewId, dto);
            return Ok();
        }

        [Authorize]
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> Delete(Guid reviewId)
        {
            await _reviewService.DeleteAsync(reviewId);
            return NoContent();
        }
    }

}
