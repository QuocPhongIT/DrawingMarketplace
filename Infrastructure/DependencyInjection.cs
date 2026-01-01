using CloudinaryDotNet;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Application.Services;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using DrawingMarketplace.Infrastructure.Repositories;
using DrawingMarketplace.Infrastructure.Services;
using DrawingMarketplace.Infrastructure.Services.PaymentGateways;
using DrawingMarketplace.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DrawingMarketplace.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("DATABASE_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "DATABASE_CONNECTION is not configured. Please set it in Render Environment Variables.");
            }

            services.AddDbContext<DrawingMarketplaceContext>(options =>
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

                dataSourceBuilder.MapEnum<CollaboratorRequestStatus>("collaborator_request_status");
                dataSourceBuilder.MapEnum<CollaboratorActivityStatus>("collaborator_activity_status");
                dataSourceBuilder.MapEnum<UserStatus>("user_status");
                dataSourceBuilder.MapEnum<OtpType>("otp_type");
                dataSourceBuilder.MapEnum<ContentStatus>("content_status");
                dataSourceBuilder.MapEnum<FilePurpose>("file_purpose");
                dataSourceBuilder.MapEnum<OrderStatus>("order_status");
                dataSourceBuilder.MapEnum<PaymentStatus>("payment_status");
                dataSourceBuilder.MapEnum<WithdrawalStatus>("withdrawal_status");
                dataSourceBuilder.MapEnum<WalletOwnerType>("wallet_owner_type");
                dataSourceBuilder.MapEnum<WalletTxType>("wallet_tx_type");
                dataSourceBuilder.MapEnum<CouponType>("coupon_type");


                options
                    .UseNpgsql(dataSourceBuilder.Build())
                    .EnableSensitiveDataLogging(false)
                    .LogTo(Console.WriteLine, LogLevel.Information);
            });

            var cloudinarySettings = configuration.GetSection("Cloudinary");
            var cloudName = cloudinarySettings["CloudName"];
            var apiKey = cloudinarySettings["ApiKey"];
            var apiSecret = cloudinarySettings["ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) ||
                string.IsNullOrEmpty(apiKey) ||
                string.IsNullOrEmpty(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary configuration is missing in environment variables.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            services.AddSingleton(new Cloudinary(account));

            services.Configure<SmtpSettings>(
                configuration.GetSection("SmtpSettings"));

            services.Configure<VnPaySettings>(
                configuration.GetSection("VnPay"));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IOtpRepository, OtpRepository>();
            services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();
            services.AddScoped<ICollaboratorRequestRepository, CollaboratorRequestRepository>();
            services.AddScoped<ICartRepository, CartRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IPaymentGateway, VnPayGateway>();

            services.AddHttpContextAccessor();

            return services;
        }
    }
}
