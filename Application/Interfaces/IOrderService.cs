using DrawingMarketplace.Application.DTOs.Order;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderDto?> GetByIdAsync(Guid id);
        Task<List<OrderDto>> GetUserOrdersAsync(Guid userId);
        Task<bool> CancelOrderAsync(Guid orderId);
        Task ProcessPaymentCallbackAsync(Guid paymentId, string status, string transactionId);
    }
}

