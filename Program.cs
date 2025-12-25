using System.Net;
using DrawingMarketplace.Api.Middlewares;
using DrawingMarketplace.Infrastructure;
using DrawingMarketplace.Application;
using DrawingMarketplace.Domain.Enums;
using DrawingMarketplace.Api.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
