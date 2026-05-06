using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// CORS - Permite TODO
builder.Services.AddCors(p => p.AddPolicy("CorsPolicy", b => 
    b.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

// Base de datos - Tu conexión exacta
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
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { ok = true }));

Console.WriteLine($"🚀 Backend ready on port {port}");
app.Run();