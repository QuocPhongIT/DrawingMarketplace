using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Exceptions;
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
            if (string.IsNullOrWhiteSpace(gatewayName))
                throw new BadRequestException("Payment gateway không ???c ?? tr?ng");

            var normalizedName = gatewayName.ToLower();

            if (!_gateways.TryGetValue(normalizedName, out var gateway))
            {
                _logger.LogWarning(
                    "Payment gateway not found: {GatewayName}", gatewayName);

                throw new BadRequestException(
                    $"Payment gateway '{gatewayName}' không ???c h? tr?");
            }

            return gateway;
        }

        public async Task<PaymentResult> CreatePaymentAsync(
     string gatewayName,
     CreatePaymentRequest request)
        {
            var gateway = GetGateway(gatewayName);

            try
            {
                return await gateway.CreatePaymentAsync(request);
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Payment gateway error: {Gateway}", gatewayName);

                throw new BadRequestException(
                    "Không th? t?o thanh toán, vui lòng th? l?i sau");
            }
        }

    }
}

