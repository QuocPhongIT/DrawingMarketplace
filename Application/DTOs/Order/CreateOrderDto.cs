using DrawingMarketplace.Application.DTOs.Cart;

namespace DrawingMarketplace.Application.DTOs.Order
{
    public class CreateOrderDto
    {
        public Guid? ContentId { get; set; }    

        public int Quantity { get; set; } = 1; 

        public string? CouponCode { get; set; } 
        public string PaymentMethod { get; set; } = "vnpay";
        //public string? ReturnUrl { get; set; }
        //public string? CancelUrl { get; set; }    
    }
}


