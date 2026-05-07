using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Puerto 8080 para Railway
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p
        .WithOrigins("*")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
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
}

builder.Services.AddControllers();

var app = builder.Build();

// ORDEN OBLIGATORIO
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ✅ HEALTH ENDPOINT - DESPUÉS de MapControllers
app.MapGet("/health", () => Results.Ok(new { ok = true, port = 8080 }));
app.MapGet("/", () => Results.Ok(new { message = "API running" }));

Console.WriteLine("🚀 Backend ready on port 8080");
app.Run();