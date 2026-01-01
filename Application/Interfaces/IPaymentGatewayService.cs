using DrawingMarketplace.Domain.Interfaces;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<PaymentResult> CreatePaymentAsync(string gatewayName, CreatePaymentRequest request);
        IPaymentGateway GetGateway(string gatewayName);
    }
}

