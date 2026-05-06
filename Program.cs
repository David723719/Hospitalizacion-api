using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Puerto para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔥 CORS para Vercel + localhost
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p
        .WithOrigins(
            "https://hospital-frontend-beryl.vercel.app",
            "http://localhost:5173",
            "https://hospitalizacion-api-production.up.railway.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// 🗄️ Base de datos - Parseo de DATABASE_URL de Railway
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

// 🔥 AUTO-FIX: Agregar columnas faltantes en la tabla Camas (si no existen)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<HospitalizacionDbContext>();
        
        // Verificar y crear tabla si no existe
        await db.Database.EnsureCreatedAsync();
        
        // Agregar columna EstadoOperativo si falta
        await db.Database.ExecuteSqlRawAsync(@"
            DO $$ 
            BEGIN 
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'Camas' AND column_name = 'EstadoOperativo'
                ) THEN 
                    ALTER TABLE ""Camas"" ADD COLUMN ""EstadoOperativo"" VARCHAR(50) DEFAULT 'Disponible'; 
                END IF;
            END $$;
        ");
        
        // Agregar columna Estado si falta
        await db.Database.ExecuteSqlRawAsync(@"
            DO $$ 
            BEGIN 
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'Camas' AND column_name = 'Estado'
                ) THEN 
                    ALTER TABLE ""Camas"" ADD COLUMN ""Estado"" VARCHAR(50) DEFAULT 'Activo'; 
                END IF;
            END $$;
        ");
        
        Console.WriteLine("✅ Tabla Camas verificada - Columnas EstadoOperativo y Estado listas");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Warning al verificar DB: {ex.Message}");
    }
}

// 🔥 Endpoint de salud para pruebas
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));

// 🔥 ORDEN CRÍTICO: CORS antes de MapControllers
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine($"🚀 Backend ready on port {port}");
app.Run();