using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 🔥 PUERTO PARA RAILWAY
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
Console.WriteLine($"🔧 Listening on port {port}");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// DB
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(dbUrl))
{
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
    Console.WriteLine($"✅ DB Connected: {cs.Host}:{cs.Port}/{cs.Database}");
}

builder.Services.AddControllers();

var app = builder.Build();

// ORDEN OBLIGATORIO
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ✅ ENDPOINT DE SALUD - DEBE IR DESPUÉS DE MapControllers
app.MapGet("/health", () => Results.Ok(new { ok = true, port = port }));
app.MapGet("/", () => Results.Ok(new { message = "Hospital API is running" }));

Console.WriteLine($"🚀 Backend ready on port {port}");
Console.WriteLine($"🌐 Health check: http://localhost:{port}/health");

app.Run();