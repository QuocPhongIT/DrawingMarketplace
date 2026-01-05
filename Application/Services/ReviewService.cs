using DrawingMarketplace.Application.DTOs.Reviews;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Exceptions;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly DrawingMarketplaceContext _context;
        private readonly ICurrentUserService _currentUser;

        public ReviewService(
            DrawingMarketplaceContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task CreateAsync(Guid contentId, CreateReviewDto dto)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var purchased = await _context.OrderItems
                .AnyAsync(oi =>
                    oi.ContentId == contentId &&
                    oi.Order.UserId == userId &&
                    oi.Order.Status == OrderStatus.paid);

            if (!purchased)
                throw new ForbiddenException("Bạn chưa mua nội dung này");

            var exists = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ContentId == contentId);

            if (exists)
                throw new BadRequestException("Bạn đã review nội dung này");

            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ContentId = contentId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid reviewId, CreateReviewDto dto)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var review = await _context.Reviews.FindAsync(reviewId)
                ?? throw new NotFoundException("Review", reviewId);

            if (review.UserId != userId)
                throw new ForbiddenException("Không thể sửa review của người khác");

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid reviewId)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var review = await _context.Reviews.FindAsync(reviewId)
                ?? throw new NotFoundException("Review", reviewId);

            if (review.UserId != userId)
                throw new ForbiddenException();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ReviewDto>> GetByContentAsync(Guid contentId)
        {
            return await _context.Reviews
                .Where(r => r.ContentId == contentId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }
    }

}
