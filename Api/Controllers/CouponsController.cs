using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Coupon;
using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DrawingMarketplace.Application.DTOs.Coupon.CouponUpsertDto;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/coupons")]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var coupons = await _couponService.GetAllAsync();
            return this.Success(coupons, "Lấy danh sách coupon thành công", "Get coupons successfully");
        }
        [AllowAnonymous]
        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var coupon = await _couponService.GetByCodeAsync(code);
            if (coupon == null)
                return this.NotFound("Coupon", "Coupon not found");

            return this.Success(coupon, "Lấy thông tin coupon thành công", "Get coupon successfully");
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
        {
            var coupon = await _couponService.CreateAsync(dto);
            return this.Success(coupon, "Tạo coupon thành công", "Create coupon successfully", 201);
        }
        [Authorize(Roles = "admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponDto dto)
        {
            var coupon = await _couponService.UpdateAsync(id, dto);
            if (coupon == null)
                return this.NotFound("Coupon", "Coupon not found");

            return this.Success(coupon, "Cập nhật coupon thành công", "Update coupon successfully");
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _couponService.DeleteAsync(id);
            if (!result)
                return this.NotFound("Coupon", "Coupon not found");

            return this.Success<object>(null, "Xóa coupon thành công", "Delete coupon successfully");
        }
    }
}

