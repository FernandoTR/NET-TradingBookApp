using Application.Common;
using Application.Interfaces;
using Infrastructure.Persistence.Data;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Web.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.Configure<AiTradeValidationOptions>(
            builder.Configuration.GetSection(AiTradeValidationOptions.SectionName));

        builder.Services.AddScoped<IAiValidationImageValidator, AiValidationImageValidator>();
        builder.Services.AddScoped<AiValidationImageRequestHandler>();

        //builder.Services.AddScoped<IUser, CurrentUser>();       

        // Agrega una verificación de la conexión a la base de datos a través del ApplicationDbContext
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.AddPolicy("AccountPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromSeconds(5)
                    }));
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync("Demasiadas solicitudes. Intente más tarde.", cancellationToken);
            };
        });

    }
}
