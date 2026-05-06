using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 🌐 Puerto dinámico para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔒 CORS: Permite frontend local + Vercel + Railway (ULTRA PERMISIVO para desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)  // ← Permite CUALQUIER origen (desarrollo)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 🗄️ Base de datos CON MANEJO DE ERRORES
try
{
    var connectionString = ResolveConnectionString(builder.Configuration);
    builder.Services.AddDbContext<HospitalizacionDbContext>(options =>
        options.UseNpgsql(connectionString));
    Console.WriteLine($"✅ DB configurada correctamente");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERROR CONFIGURANDO DB: {ex.Message}");
    // No lanzar excepción para que el backend arranque igual y podamos ver el error en logs
}

// 📦 Controllers + Swagger
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hospitalización API", Version = "v1" });
});

var app = builder.Build();

// 🔄 Logging middleware para debug
app.Use(async (context, next) =>
{
    Console.WriteLine($"📥 {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"📤 {context.Response.StatusCode}");
});

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

app.UseCors("AllowAll");  // ← Nombre debe coincidir EXACTAMENTE con AddCors
app.UseAuthorization();
app.MapControllers();

Console.WriteLine($"🚀 Backend listo en puerto {port}");
Console.WriteLine($"✅ Environment: {app.Environment.EnvironmentName}");

app.Run();

// 🔧 Resolver conexión: Railway (DATABASE_URL) o Local (appsettings.json)
static string ResolveConnectionString(IConfiguration config)
{
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    
    if (!string.IsNullOrWhiteSpace(dbUrl))
    {using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 🌐 Puerto dinámico para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔒 CORS: Permite frontend local + Vercel + Railway (ULTRA PERMISIVO para desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)  // ← Permite CUALQUIER origen (desarrollo)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 🗄️ Base de datos CON MANEJO DE ERRORES
try
{
    var connectionString = ResolveConnectionString(builder.Configuration);
    builder.Services.AddDbContext<HospitalizacionDbContext>(options =>
        options.UseNpgsql(connectionString));
    Console.WriteLine($"✅ DB configurada correctamente");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERROR CONFIGURANDO DB: {ex.Message}");
    // No lanzar excepción para que el backend arranque igual y podamos ver el error en logs
}

// 📦 Controllers + Swagger
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hospitalización API", Version = "v1" });
});

var app = builder.Build();

// 🔄 Logging middleware para debug
app.Use(async (context, next) =>
{
    Console.WriteLine($"📥 {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"📤 {context.Response.StatusCode}");
});

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

app.UseCors("AllowAll");  // ← Nombre debe coincidir EXACTAMENTE con AddCors
app.UseAuthorization();
app.MapControllers();

Console.WriteLine($"🚀 Backend listo en puerto {port}");
Console.WriteLine($"✅ Environment: {app.Environment.EnvironmentName}");

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
            
            Console.WriteLine($"✅ Conectando a PostgreSQL: {uri.Host}:{uri.Port}/{database}");
            return npgsql.ConnectionString;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error parseando DATABASE_URL: {ex.Message}");
            throw;
        }
    }
    
    var local = config.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(local))
    {
        Console.WriteLine($"✅ Conectando a PostgreSQL (local)");
        return local;
    }
    
    throw new InvalidOperationException("❌ No connection string found. Set DATABASE_URL (prod) or DefaultConnection (local).");
}