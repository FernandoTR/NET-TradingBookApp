
using Application.Interfaces;
using Application.Services;
using Infrastructure;
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
        builder.Services.AddScoped<ICatFigureService, CatFigureService>();
        builder.Services.AddScoped<ICatCategoryService, CatCategoryService>();
        builder.Services.AddScoped<ICatAccountTypeService, CatAccountTypeService>();
        builder.Services.AddScoped<ICatFrameService, CatFrameService>();
        builder.Services.AddScoped<ICatInstrumentsService, CatInstrumentsService>();
        builder.Services.AddScoped<IAccountsService, AccountsService>();
        builder.Services.AddScoped<IAccountBalancesService, AccountBalancesService>();






    }
}

