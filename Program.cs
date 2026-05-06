using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using HospitalizacionAPI.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 🌐 Solo escuchar en puerto dinámico (Railway lo asigna)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔒 CORS SEGURO: Diferente para desarrollo y producción
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // ✅ Desarrollo: permite todo (localhost, red local)
            policy.SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost") ||
                origin.StartsWith("http://127.0.0.1") ||
                origin.StartsWith("http://10.77.") ||
                origin.StartsWith("http://192.168.") ||
                origin.StartsWith("http://172."))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }
        else
        {
            // 🔒 Producción: Solo orígenes específicos
            var allowedOrigins = builder.Configuration
                .GetSection("Security:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();
            
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// 📦 HttpClient + Configuración Externa
builder.Services.AddHttpClient();
builder.Services.Configure<ExternalServicesConfig>(builder.Configuration.GetSection("ExternalServices"));
builder.Services.AddScoped<ExternalApiService>();

// 🗄️ Base de datos (usa variable de entorno en Railway)
var connectionString = ResolveConnectionString(builder.Configuration);

builder.Services.AddDbContext<HospitalizacionDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Hospitalización API", 
        Version = "v1",
        Description = "API para gestión de admisiones y camas"
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    try
    {
        var db = services.GetRequiredService<HospitalizacionDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error aplicando migraciones al iniciar.");
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

Console.WriteLine($"✅ Backend listo en puerto {port}");
Console.WriteLine($"✅ Environment: {app.Environment.EnvironmentName}");

app.Run();

static string ResolveConnectionString(IConfiguration configuration)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        // Railway entrega conexión tipo URL: postgresql://user:pass@host:port/db
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = userInfo.Length > 0 ? userInfo[0] : string.Empty;
        var password = userInfo.Length > 1 ? userInfo[1] : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Username = Uri.UnescapeDataString(username),
            Password = Uri.UnescapeDataString(password),
            Database = database,
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }

    return configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("No se encontró la cadena de conexión.");
}