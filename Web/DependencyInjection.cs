using Infrastructure.Persistence.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        //builder.Services.AddScoped<IUser, CurrentUser>();       

        // Agrega una verificación de la conexión a la base de datos a través del ApplicationDbContext
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

       

    }
}
