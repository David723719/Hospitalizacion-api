using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using HospitalizacionAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Puerto para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔥 CORS - PERMITE TODO
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// 🗄️ Base de datos - Parseo DIRECTO de tu DATABASE_URL
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(dbUrl))
{
    // Tu URL: postgresql://postgres:PASS@trolley.proxy.rlwy.net:54033/railway
    var uri = new Uri(dbUrl);
    var userPass = uri.UserInfo.Split(':');
    
    var cs = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,                        // trolley.proxy.rlwy.net
        Port = uri.Port,                        // 54033
        Username = userPass[0],                 // postgres
        Password = userPass[1],                 // mBnVjAqqXptogpKsefIaIFEWaObrAuPq
        Database = uri.AbsolutePath.Trim('/'),  // railway
        SslMode = SslMode.Require               // Railway requiere SSL
    };
    
    builder.Services.AddDbContext<HospitalizacionDbContext>(o => o.UseNpgsql(cs.ConnectionString));
    Console.WriteLine($"✅ DB OK: {cs.Host}:{cs.Port}/{cs.Database}");
}
else
{
    builder.Services.AddDbContext<HospitalizacionDbContext>(o => 
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hospital API", Version = "v1" }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "API"); c.RoutePrefix = "swagger"; });
}

// 🔥 ORDEN CRÍTICO: CORS antes de MapControllers
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine($"🚀 Backend ready on port {port}");
app.Run();