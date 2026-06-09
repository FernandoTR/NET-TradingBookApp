using Microsoft.Extensions.Options;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.
    builder.AddApplicationServices();
    builder.AddInfrastructureServices();
    builder.AddWebServices();

    builder.Services.AddRazorPages();
    builder.Services.AddControllersWithViews();

    // Configurar la zona horaria predeterminada
    //var mexicoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
    //TimeZoneInfo.Local = mexicoTimeZone;

    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new[] { "es-MX" };
        options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("es-MX");
        options.SupportedCultures = supportedCultures.Select(c => new System.Globalization.CultureInfo(c)).ToList();
        options.SupportedUICultures = options.SupportedCultures;
    });

    var app = builder.Build();

    app.UseRequestLocalization();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseSerilogRequestLogging();

    app.UseRouting();

    // Usar autenticacion y autorizacion
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
