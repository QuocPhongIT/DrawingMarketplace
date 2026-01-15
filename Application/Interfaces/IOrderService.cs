using DrawingMarketplace.Application.DTOs.Order;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderDto?> GetByIdAsync(Guid id);
        Task<List<OrderDto>> GetUserOrdersAsync(Guid userId);
        Task<bool> CancelOrderAsync(Guid orderId);

        Task ProcessPaymentIpnAsync(
            Guid paymentId,
            string responseCode,
            string transactionNo,
            Dictionary<string, string> rawData
        );
    }
}
