namespace DrawingMarketplace.Application.DTOs.Order
{
    public class CreateOrderDto
    {
        public string? CouponCode { get; set; }
        public string PaymentMethod { get; set; } = "vnpay"; // vnpay, momo
        public decimal TotalAmount { get; set; }
    }
}


