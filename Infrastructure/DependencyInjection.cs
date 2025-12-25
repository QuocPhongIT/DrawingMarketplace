using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Domain.Interfaces;
using DrawingMarketplace.Infrastructure.Persistence;
using DrawingMarketplace.Infrastructure.Repositories;
using DrawingMarketplace.Infrastructure.Services;
using DrawingMarketplace.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace DrawingMarketplace.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                                   ?? configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DrawingMarketplaceContext>(options =>
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

                dataSourceBuilder.MapEnum<CollaboratorRequestStatus>("collaborator_request_status");
                dataSourceBuilder.MapEnum<CollaboratorActivityStatus>("collaborator_activity_status");
                dataSourceBuilder.MapEnum<UserStatus>("user_status");
                dataSourceBuilder.MapEnum<OtpType>("otp_type");
                dataSourceBuilder.MapEnum<ContentStatus>("content_status");

                var dataSource = dataSourceBuilder.Build();

                options
                    .UseNpgsql(dataSource)
                    .EnableSensitiveDataLogging()
                    .LogTo(Console.WriteLine, LogLevel.Information);
            });

            services.Configure<SmtpSettings>(
                configuration.GetSection("SmtpSettings"));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IOtpRepository, OtpRepository>();
            services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();
            services.AddScoped<ICollaboratorRequestRepository, CollaboratorRequestRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
