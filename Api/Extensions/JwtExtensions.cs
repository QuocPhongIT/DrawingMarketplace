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
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                     ?? configuration["Jwt:Key"];
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                        ?? configuration["Jwt:Issuer"];
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                          ?? configuration["Jwt:Audience"];

        var key = Encoding.UTF8.GetBytes(jwtKey!);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = jwtAudience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine("JWT ERROR: " + ctx.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}
