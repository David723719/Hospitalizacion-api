using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Puerto
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔥 CORS - PERMISIVO Y FUNCIONAL
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .WithOrigins(
                "https://hospital-frontend-beryl.vercel.app",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()  // GET, POST, PUT, DELETE, OPTIONS
            .AllowCredentials();
    });
});

// Base de datos
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? "postgresql://postgres:mBnVjAqqXptogpKsefIaIFEWaObrAuPq@trolley.proxy.rlwy.net:54033/railway";

var uri = new Uri(dbUrl);
var up = uri.UserInfo.Split(':');
var cs = new NpgsqlConnectionStringBuilder
{
    Host = uri.Host, Port = uri.Port,
    Username = up[0], Password = up[1],
    Database = uri.AbsolutePath.Trim('/'),
    SslMode = SslMode.Require
};

builder.Services.AddDbContext<HospitalizacionDbContext>(o => o.UseNpgsql(cs.ConnectionString));
builder.Services.AddControllers();

var app = builder.Build();

// 🔥 ORDEN CRÍTICO: CORS ANTES de Authorization y MapControllers
app.UseCors("AllowAll");        // ← Nombre debe coincidir con AddCors
app.UseAuthorization();
app.MapControllers();

// Health check
app.MapGet("/health", () => Results.Ok(new { ok = true }));

Console.WriteLine($"🚀 Backend ready on port {port}");
app.Run();