namespace DrawingMarketplace.Application.DTOs.Order
{
    public class CouponDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }     
        public bool IsPercentage { get; set; }    
        public decimal? MaxDiscountAmount { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}


