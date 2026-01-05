using DrawingMarketplace.Application.DTOs.Reviews;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IReviewService
    {
        Task CreateAsync(Guid contentId, CreateReviewDto dto);
        Task UpdateAsync(Guid reviewId, CreateReviewDto dto);
        Task DeleteAsync(Guid reviewId);
        Task<List<ReviewDto>> GetByContentAsync(Guid contentId);
    }
}
