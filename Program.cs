using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using HospitalizacionAPI.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 🌐 Puerto dinámico para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔒 CORS: Permite frontend local + Vercel + Railway
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",      // Frontend Vite local
            "http://localhost:5200",      // Backend local
            "https://hospital-frontend-david723719.vercel.app", // Frontend en Vercel
            "https://hospitalizacion-api-production.up.railway.app" // Backend en Railway
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// 📦 Servicios
builder.Services.AddHttpClient();
builder.Services.Configure<ExternalServicesConfig>(builder.Configuration.GetSection("ExternalServices"));
builder.Services.AddScoped<ExternalApiService>();

// 🗄️ Base de datos
var connectionString = ResolveConnectionString(builder.Configuration);
builder.Services.AddDbContext<HospitalizacionDbContext>(options =>
    options.UseNpgsql(connectionString));

// 📦 Controllers + Swagger
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Hospitalización API", 
        Version = "v1",
        Description = "API para gestión de admisiones, camas y reportes"
    });
});

var app = builder.Build();

// 🔄 Inicializar DB
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        var db = services.GetRequiredService<HospitalizacionDbContext>();
        logger.LogInformation("✅ DB context initialized");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "⚠️ DB init warning");
    }
}

// 🧭 Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"); c.RoutePrefix = "swagger"; });
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // ← DEBE coincidir con el nombre de la política
app.UseAuthorization();
app.MapControllers();

Console.WriteLine($"✅ Backend: http://0.0.0.0:{port}");
Console.WriteLine($"✅ Environment: {app.Environment.EnvironmentName}");

app.Run();

// 🔧 Resolver conexión: Railway o Local
static string ResolveConnectionString(IConfiguration config)
{
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(dbUrl))
    {
        var uri = new Uri(dbUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo.Length > 0 ? userInfo[0] : "");
        var password = Uri.UnescapeDataString(userInfo.Length > 1 ? userInfo[1] : "");
        var database = uri.AbsolutePath.TrimStart('/');
        var npgsql = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host, Port = uri.Port, Username = username, Password = password,
            Database = database, SslMode = SslMode.Require, Timeout = 30, CommandTimeout = 60
        };
        return npgsql.ConnectionString;
    }
    var local = config.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(local)) return local;
    throw new InvalidOperationException("❌ No connection string found.");
}