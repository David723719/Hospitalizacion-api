using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p
        .WithOrigins("https://hospital-frontend-beryl.vercel.app", "http://localhost:5173")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

// Base de datos
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
        SslMode = SslMode.Require, Timeout = 30
    };
    builder.Services.AddDbContext<HospitalizacionDbContext>(o => o.UseNpgsql(cs.ConnectionString));
    Console.WriteLine($"✅ DB Config: {cs.Host}:{cs.Port}/{cs.Database}");
}
else
{
    builder.Services.AddDbContext<HospitalizacionDbContext>(o => 
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddControllers();
var app = builder.Build();

// 🔥 DEBUG: Endpoint para ver estado real de la DB
app.MapGet("/api/debug", async (HospitalizacionDbContext db) =>
{
    try
    {
        // Verificar conexión
        await db.Database.CanConnectAsync();
        
        // Listar tablas existentes
        var tables = await db.Database.SqlQueryRaw<string>(
            @"SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'"
        ).ToListAsync();
        
        // Verificar columnas de Camas
        var columns = await db.Database.SqlQueryRaw<string>(
            @"SELECT column_name FROM information_schema.columns WHERE table_name = 'Camas'"
        ).ToListAsync();
        
        return Results.Ok(new { 
            connected = true, 
            tables, 
            camasColumns = columns,
            message = "DB OK - Revisa camasColumns para ver si falta EstadoOperativo"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ DEBUG ERROR: {ex.Message}");
        Console.WriteLine($"❌ Stack: {ex.StackTrace}");
        return Results.Ok(new { connected = false, error = ex.Message, stack = ex.StackTrace });
    }
});

// 🔥 LOGGING FORZADO: Middleware que registra TODO
app.Use(async (context, next) =>
{
    Console.WriteLine($"📥 {context.Request.Method} {context.Request.Path}");
    try
    {
        await next();
        Console.WriteLine($"📤 {context.Response.StatusCode}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌❌❌ UNHANDLED EXCEPTION ❌❌❌");
        Console.WriteLine($"Path: {context.Request.Path}");
        Console.WriteLine($"Message: {ex.Message}");
        Console.WriteLine($"Stack: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner: {ex.InnerException.Message}");
            Console.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
        }
        throw;
    }
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine($"🚀 Backend ready on port {port}");
app.Run();