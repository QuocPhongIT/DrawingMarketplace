using DrawingMarketplace.Application.Common.Services;
using DrawingMarketplace.Application.Features.Auth;
using DrawingMarketplace.Application.Features.Cart;
using DrawingMarketplace.Application.Features.Collaborators;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Application.Profiles;
using DrawingMarketplace.Application.Services;

namespace DrawingMarketplace.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<RegisterHandler>();
            services.AddScoped<LoginHandler>();
            services.AddScoped<VerifyOtpHandler>();
            services.AddScoped<ResendOtpHandler>();
            services.AddScoped<ForgotPasswordHandler>();
            services.AddScoped<ResetPasswordHandler>();
            services.AddScoped<VerifyResetPasswordOtpHandler>();
            services.AddScoped<LogoutHandler>();
            services.AddScoped<RefreshTokenHandler>();
            services.AddScoped<LogoutAllDevicesHandler>();
            services.AddScoped<ApplyCollaboratorHandler>();
            services.AddScoped<ApproveCollaboratorHandler>();
            services.AddScoped<RejectCollaboratorHandler>();
            services.AddScoped<AddToCartHandler>();
            services.AddScoped<RemoveCartHandler>();
            services.AddScoped<GetCartQueryHandler>();
            services.AddScoped<ClearCartHandler>();

            services.AddScoped(typeof(ICrudService<,,,>), typeof(CrudService<,,,>));
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IContentService, ContentService>();
            services.AddScoped<IBannerService, BannerService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IWithdrawalService, WithdrawalService>();
            services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
            services.AddScoped<IDownloadService, DownloadService>();
            services.AddScoped<IReviewService, ReviewService>();


            services.AddAutoMapper(typeof(MappingProfile).Assembly);


            return services;
        }
    }
}
