
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // Registrar el servicio de mensajes de validación
        builder.Services.AddTransient<IMessageService, MessageService>();


        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
        builder.Services.AddScoped<IMenuService, MenuService>();
        builder.Services.AddScoped<IRolesService, RolesService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
        builder.Services.AddScoped<IStringUtilitiesService, StringUtilitiesService>();






    }
}

