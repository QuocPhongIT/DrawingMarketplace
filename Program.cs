using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Api.Middlewares;
using DrawingMarketplace.Application;
using DrawingMarketplace.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using NetEscapades.AspNetCore.SecurityHeaders;
using NetEscapades.AspNetCore.SecurityHeaders.Headers;
using DotNetEnv;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using DrawingMarketplace.Infrastructure.Settings;
using Serilog;
using Serilog.Events;

Env.Load();

AppContext.SetSwitch("Npgsql.EnableLegacyNamingConvention", false);

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()                           
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)  
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .WriteTo.Console()                           
    .CreateBootstrapLogger();

builder.Host.UseSerilog();

builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<CloudinaryConfig>(builder.Configuration.GetSection("Cloudinary"));
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionCors", policy =>
    {
        policy.WithOrigins("https://ban-ve-app.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ApiPolicy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    options.RejectionStatusCode = 429;
    options.OnRejected = (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/json";
        context.HttpContext.Response.WriteAsync("{\"error\":\"Quá nhiều request. Vui lòng thử lại sau.\"}", cancellationToken: token);
        return new ValueTask();
    };
});

builder.Services.AddSecurityHeaderPolicies()
    .SetDefaultPolicy(policy =>
    {
        policy.AddFrameOptionsDeny();
        policy.AddXssProtectionBlock();
        policy.AddContentTypeOptionsNoSniff();
        policy.AddReferrerPolicyStrictOriginWhenCrossOrigin();
        policy.RemoveServerHeader();
        policy.AddCrossOriginOpenerPolicy(builder => builder.SameOrigin());
        policy.AddCrossOriginEmbedderPolicy(builder => builder.RequireCorp());
        policy.AddCrossOriginResourcePolicy(builder => builder.SameOrigin());
        policy.AddPermissionsPolicy(builder =>
        {
            builder.AddAccelerometer().None();
            builder.AddCamera().None();
            builder.AddGeolocation().None();
            builder.AddGyroscope().None();
            builder.AddMagnetometer().None();
            builder.AddMicrophone().None();
            builder.AddPayment().None();
        });
    });

builder.WebHost.ConfigureKestrel(options =>
{
    var port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "8080");
    options.ListenAnyIP(port);
});

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
    };
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseSecurityHeaders();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCors");
}
else
{
    app.UseCors("ProductionCors");
}

app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();