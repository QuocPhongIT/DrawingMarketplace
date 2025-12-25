using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace DrawingMarketplace.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {your JWT token}"
            };

            c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        });

        return services;
    }
}
