using DrawingMarketplace.Application.DTOs.Reviews;
using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

        [SwaggerOperation(
            Summary = "Tạo đánh giá",
            Description = "Người dùng tạo đánh giá cho một nội dung đã mua"
        )]
        [Authorize]
        [HttpPost("contents/{contentId}")]
        public async Task<IActionResult> Create(Guid contentId, CreateReviewDto dto)
        {
            await _reviewService.CreateAsync(contentId, dto);
            return Ok();
        }

        [SwaggerOperation(
            Summary = "Lấy danh sách đánh giá theo nội dung",
            Description = "Lấy tất cả đánh giá của một content"
        )]
        [HttpGet("contents/{contentId}")]
        public async Task<IActionResult> GetByContent(Guid contentId)
        {
            return Ok(await _reviewService.GetByContentAsync(contentId));
        }

        [SwaggerOperation(
          Summary = "Cập nhật đánh giá",
          Description = "Người dùng cập nhật nội dung đánh giá của mình"
        )]
        [Authorize]
        [HttpPut("{reviewId}")]
        public async Task<IActionResult> Update(Guid reviewId, CreateReviewDto dto)
        {
            await _reviewService.UpdateAsync(reviewId, dto);
            return Ok();
        }

        [SwaggerOperation(
            Summary = "Xóa đánh giá",
            Description = "Người dùng xóa đánh giá của mình"
        )]
        [Authorize]
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> Delete(Guid reviewId)
        {
            await _reviewService.DeleteAsync(reviewId);
            return NoContent();
        }
    }

}
