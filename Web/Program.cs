using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

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
    var supportedCultures = new[] { "es-MX" }; // Especï¿½fico para Mï¿½xico
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("es-MX");
    options.SupportedCultures = supportedCultures.Select(c => new System.Globalization.CultureInfo(c)).ToList();
    options.SupportedUICultures = options.SupportedCultures;
});

var app = builder.Build();

app.UseRequestLocalization(); // Usar la configuraciï¿½n de localizaciï¿½n

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Usar autenticaciÃ³n y autorizaciÃ³n
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();


app.Run();