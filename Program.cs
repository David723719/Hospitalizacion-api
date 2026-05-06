using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using HospitalizacionAPI.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 🌐 Puerto dinámico para Railway/Render/Vercel
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔒 CORS: Permite TODOS los dominios de tus microservicios + desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            var o = origin.ToLowerInvariant();
            return o.Contains("localhost") ||
                   o.Contains("127.0.0.1") ||
                   o.Contains("vercel.app") ||
                   o.Contains("railway.app") ||
                   o.Contains("onrender.com") ||
                   o.Contains("ngrok") ||
                   o.StartsWith("http://10.77.") ||
                   o.StartsWith("http://192.168.") ||
                   o.StartsWith("http://172.");
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// 📦 HttpClient + Configuración de servicios externos
builder.Services.AddHttpClient();
builder.Services.Configure<ExternalServicesConfig>(builder.Configuration.GetSection("ExternalServices"));
builder.Services.AddScoped<ExternalApiService>();

// 🗄️ Base de datos PostgreSQL (Railway DATABASE_URL o local appsettings.json)
var connectionString = ResolveConnectionString(builder.Configuration);
builder.Services.AddDbContext<HospitalizacionDbContext>(options =>
    options.UseNpgsql(connectionString));

// 📦 Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Hospitalización API", 
        Version = "v1",
        Description = "API para gestión de admisiones, camas y reportes MIS"
    });
});

var app = builder.Build();

// 🔄 Inicializar DB (sin lanzar excepción para no romper deploy en Railway)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        var db = services.GetRequiredService<HospitalizacionDbContext>();
        // db.Database.Migrate(); // ← Descomenta si necesitas migraciones automáticas
        logger.LogInformation("✅ DB context initialized");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "⚠️ DB init warning (non-fatal)");
    }
}

// 🧭 Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hospitalización API v1");
        c.RoutePrefix = "swagger";
    });
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine($"✅ Backend: http://0.0.0.0:{port}");
Console.WriteLine($"✅ Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"✅ Swagger: /swagger");

app.Run();

// 🔧 Resolver conexión: Railway (DATABASE_URL) o Local (appsettings.json)
static string ResolveConnectionString(IConfiguration config)
{
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    
    if (!string.IsNullOrWhiteSpace(dbUrl))
    {
        try
        {
            var uri = new Uri(dbUrl);
            var userInfo = uri.UserInfo.Split(':', 2);
            var username = Uri.UnescapeDataString(userInfo.Length > 0 ? userInfo[0] : "");
            var password = Uri.UnescapeDataString(userInfo.Length > 1 ? userInfo[1] : "");
            var database = uri.AbsolutePath.TrimStart('/');

            var npgsql = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Username = username,
                Password = password,
                Database = database,
                SslMode = SslMode.Require,
                Timeout = 30,
                CommandTimeout = 60
            };
            Console.WriteLine($"✅ DB: {uri.Host}:{uri.Port}/{database}");
            return npgsql.ConnectionString;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ DB URL error: {ex.Message}");
            throw;
        }
    }

    var local = config.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(local))
    {
        Console.WriteLine($"✅ DB Local: [CONFIGURADO LOCALMENTE]");
        return local;
    }

    throw new InvalidOperationException("❌ No connection string found. Set DATABASE_URL (prod) or DefaultConnection (local).");
}