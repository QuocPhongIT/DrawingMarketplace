using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Interfaces;

namespace DrawingMarketplace.Application.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly Dictionary<string, IPaymentGateway> _gateways;
        private readonly ILogger<PaymentGatewayService> _logger;

        public PaymentGatewayService(
            IEnumerable<IPaymentGateway> gateways,
            ILogger<PaymentGatewayService> logger)
        {
            _logger = logger;
            _gateways = gateways.ToDictionary(
                g => g.GetType().Name.Replace("Gateway", "").ToLower(),
                g => g
            );
        }

        public IPaymentGateway GetGateway(string gatewayName)
        {
            var normalizedName = gatewayName.ToLower();
            if (!_gateways.TryGetValue(normalizedName, out var gateway))
            {
                throw new ArgumentException($"Payment gateway '{gatewayName}' not found");
            }
            return gateway;
        }

        public async Task<PaymentResult> CreatePaymentAsync(string gatewayName, CreatePaymentRequest request)
        {
            var gateway = GetGateway(gatewayName);
            return await gateway.CreatePaymentAsync(request);
        }
    }
}

