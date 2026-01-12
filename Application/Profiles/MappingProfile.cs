using AutoMapper;
using DrawingMarketplace.Application.DTOs.Cart;
using DrawingMarketplace.Application.DTOs.Catogory;
using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.DTOs.CopyrightReport;
using DrawingMarketplace.Application.DTOs.Order;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Category, CategoryDto>().ReverseMap();

            CreateMap<Category, CategoryUpsertDto.CreateCategoryDto>().ReverseMap();
            CreateMap<Category, CategoryUpsertDto.UpdateCategoryDto>().ReverseMap();

            CreateMap<ContentUpsertDto.CreateContentDto, Content>();
            CreateMap<ContentUpsertDto.UpdateContentDto, Content>();
            CreateMap<CartItem, CartItemDto>().ReverseMap();
            CreateMap<Order, OrderDto>()
               .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
               .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment))
               .ForMember(dest => dest.Coupon, opt => opt.MapFrom(src => src.OrderCoupon));

            CreateMap<OrderItem, OrderItemDto>().ReverseMap();

            CreateMap<Payment, PaymentDto>();
            CreateMap<OrderCoupon, CouponDto>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Coupon.Code))
                .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount));
            CreateMap<ContentListDto, ContentStatsAdminDto>();
            CreateMap<ContentListDto, ContentStatsCollaboratorDto>();
        }
    }
}
