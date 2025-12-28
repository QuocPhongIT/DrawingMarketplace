using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DrawingMarketplace.Api.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey =
            Environment.GetEnvironmentVariable("JWT_KEY")
            ?? configuration["Jwt:Key"];

        var jwtIssuer =
            Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? configuration["Jwt:Issuer"];

        var jwtAudience =
            Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? configuration["Jwt:Audience"];

        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("JWT_KEY is not configured.");

        if (Encoding.UTF8.GetBytes(jwtKey).Length < 32)
            throw new InvalidOperationException("JWT_KEY must be at least 256 bits (32 bytes).");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = jwtAudience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT AUTH FAILED: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}
