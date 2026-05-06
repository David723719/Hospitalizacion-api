using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 1. Puerto para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 2. 🔥 CONFIGURACIÓN CORS COMPLETA
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            // Permite los dominios de Vercel (producción y preview) y localhost
            .WithOrigins(
                "https://hospital-frontend-beryl.vercel.app",
                "https://hospital-frontend-44lke7rkt-david723719s-projects.vercel.app",
                "http://localhost:5173",
                "https://hospitalizacion-api-production.up.railway.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()  // GET, POST, PUT, DELETE, OPTIONS
            .AllowCredentials();
    });
});

// 3. Base de datos - Parseo de DATABASE_URL de Railway
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(dbUrl))
{
    var uri = new Uri(dbUrl);
    var up = uri.UserInfo.Split(':');
    var cs = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = up[0],
        Password = up[1],
        Database = uri.AbsolutePath.Trim('/'),
        SslMode = SslMode.Require,
        Timeout = 30
    };
    builder.Services.AddDbContext<HospitalizacionDbContext>(o => o.UseNpgsql(cs.ConnectionString));
    Console.WriteLine($"✅ DB: {cs.Host}:{cs.Port}/{cs.Database}");
}
else
{
    builder.Services.AddDbContext<HospitalizacionDbContext>(o => 
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddControllers();
var app = builder.Build();

// 🔥 DEBUG: Endpoint para verificar DB (puedes acceder a /api/debug)
app.MapGet("/api/debug", async (HospitalizacionDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { connected = true, message = "Backend y DB conectados" });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { connected = false, error = ex.Message });
    }
});

// 4. ORDEN CRÍTICO DE MIDDLEWARES
// CORS debe ir ANTES de Authorization y MapControllers
app.UseCors("AllowAll");       
app.UseAuthorization();        
app.MapControllers();          

Console.WriteLine($"🚀 Backend ready on port {port}");
app.Run();