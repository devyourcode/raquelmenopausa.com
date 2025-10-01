using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RaquelMenopausa.Cms.Helpers;
using RaquelMenopausa.Cms.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine("Connection String: " + connectionString); // para debug

builder.Services.AddDbContext<Context>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 29)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure());

    // Ativa o log de SQL detalhado
    options.EnableSensitiveDataLogging();  // mostra os parâmetros reais nas queries
    options.LogTo(Console.WriteLine, LogLevel.Information); // envia o log para o console
});

// Força URLs minúsculas
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = false;
});

builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("CMS", client =>
{
    client.BaseAddress = new Uri("https://yourcode.raquelmenopausa.net");
});

builder.Services.AddScoped<CmsService>();


#region autenticação
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.IsEssential = true;
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(30); //30 dias
        options.LoginPath = "/Login";   // Redireciona para essa tela se não estiver logado
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login/acesso-negado";
    });
#endregion

#region logs
Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Filter.ByExcluding(logEvent =>
        logEvent.Properties.ContainsKey("SourceContext") &&
        logEvent.Properties["SourceContext"].ToString().StartsWith("\"Microsoft")) // Filtra logs do ASP.NET Core
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.MySQL(connectionString, "logs")
    .CreateLogger();

builder.Host.UseSerilog();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Se o admin só será acessado de IPs conhecidos
//app.Use(async (context, next) =>
//{
//    var ip = context.Connection.RemoteIpAddress?.ToString();
//    if (context.Request.Path.StartsWithSegments("/Admin") && ip != "192.168.0.100")
//    {
//        context.Response.StatusCode = 403;
//        await context.Response.WriteAsync("Acesso negado.");
//        return;
//    }
//    await next();
//});

//Adicione cabeçalhos para reforçar proteção no navegador
//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Add("X-Frame-Options", "DENY");
//    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
//    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
//    await next();
//});

app.Run();
