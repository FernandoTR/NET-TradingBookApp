using Application.Interfaces;
using Domain.Constants;
using Infrastructure.Email.Services;
using Infrastructure.Identity;
using Infrastructure.Logging;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");       

        // DataBase
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddDbContext<LoggingDbContext>(options => options.UseSqlServer(connectionString));


        builder.Services.AddAuthorization();

        // Permite acceder al contexto HTTP actual sin necesidad de inyectar explícitamente el HttpContext en cada controlador o servicio.
        builder.Services.AddHttpContextAccessor();

        // Identity
        // Al configurar AddIdentity ya configura automáticamente las cookies y no se necesita llamar a AddAuthentication 
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Configuración de validación de nombres de usuario
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

            // Configuración de validación de contraseñas
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;

            // Configuración de bloqueo de usuario
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // Registra y configura los proveedores necesarios (como correo electrónico o SMS)
            options.Tokens.ProviderMap.Add("Email", new TokenProviderDescriptor(typeof(EmailTokenProvider<ApplicationUser>)));
            options.Tokens.ProviderMap.Add("Phone", new TokenProviderDescriptor(typeof(PhoneNumberTokenProvider<ApplicationUser>)));
            options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;

            // Configuración de confirmación de cuenta
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;

        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.AddScoped<IIdentityService, IdentityService>();

        //Personalización de las cookies de autenticación
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/SignIn"; // Ruta para iniciar sesión
            options.LogoutPath = "/Account/Logout"; // Ruta de cierre de sesión
            options.AccessDeniedPath = "/Account/AccessDenied"; // Ruta para acceso denegado
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Tiempo para que expire la sesión
            options.SlidingExpiration = true; // Renovar tiempo de expiración al interactuar
            options.Cookie.HttpOnly = true; // Asegura que la cookie solo sea accesible vía HTTP
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS obligatorio            
        });

        // Servicio de Correo 
        builder.Services.AddTransient<IEmailSender, EmailService>();
        builder.Services.AddTransient<IUserEmailService,UserEmailService>();

        // Repository
        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        builder.Services.AddScoped<ILogService, LogService>();
        builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        builder.Services.AddScoped<IRolesRepository, RolesRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IActivityLogsRepository, ActivityLogsRepository>();
        builder.Services.AddScoped<IMenuRepository, MenuRepository>();
        builder.Services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
        builder.Services.AddScoped<ICatDayRepository, CatDayRepository>();
        builder.Services.AddScoped<ICatDirectionRepository, CatDirectionRepository>();
        builder.Services.AddScoped<ICatFigureRepository, CatFigureRepository>();
        builder.Services.AddScoped<ICatSceneryRepository, CatSceneryRepository>();
        builder.Services.AddScoped<ICatStageRepository, CatStageRepository>();
        builder.Services.AddScoped<ICatTimeRepository, CatTimeRepository>();
        builder.Services.AddScoped<ICatTriggerRepository, CatTriggerRepository>();
        builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();


        // Servicio de Generación de códigos QR
        builder.Services.AddTransient<IQrCodeGeneratorService, QrCodeGeneratorService>();

    }
}

