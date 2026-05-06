using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración del puerto
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 2. CORS para Vercel y Localhost
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .WithOrigins(
                "https://hospital-frontend-beryl.vercel.app",
                "https://hospital-frontend-44lke7rkt-david723719s-projects.vercel.app",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 3. Conexión a la Base de Datos
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
    Console.WriteLine($"✅ DB Config OK: {cs.Host}:{cs.Port}/{cs.Database}");
}
else
{
    // Fallback local si no hay variable de entorno
    builder.Services.AddDbContext<HospitalizacionDbContext>(o => 
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddControllers();
var app = builder.Build();

// 🔥 🔥  AUTO-REPARACIÓN DE BASE DE DATOS (CRÍTICO) 🔥 🔥 🔥
// Este bloque se ejecuta al iniciar y crea la columna si falta.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HospitalizacionDbContext>();
    try
    {
        // Verificar si la columna 'EstadoOperativo' existe en la tabla 'Camas'
        var columnExists = db.Database.SqlQueryRaw<int>(@"
            SELECT COUNT(*) 
            FROM information_schema.columns 
            WHERE table_name = 'Camas' AND column_name = 'EstadoOperativo'
        ").AsEnumerable().FirstOrDefault() > 0;

        if (!columnExists)
        {
            Console.WriteLine("⚠️ Columna 'EstadoOperativo' no encontrada. Creando...");
            // Ejecutar SQL directo para agregar la columna
            db.Database.ExecuteSqlRaw(@"
                ALTER TABLE ""Camas"" 
                ADD COLUMN ""EstadoOperativo"" VARCHAR(50) DEFAULT 'Disponible'
            ");
            Console.WriteLine("✅ Columna 'EstadoOperativo' creada exitosamente.");
        }
        else
        {
            Console.WriteLine("✅ Columna 'EstadoOperativo' ya existe.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR AUTO-REPARACIÓN: {ex.Message}");
    }
}

// 4. Middleware y Rutas
app.UseCors("AllowAll"); // CORS primero
app.UseAuthorization();
app.MapControllers();

// Endpoint de prueba
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }));

Console.WriteLine($"🚀 Backend listo en puerto {port}");
app.Run();namespace HospitalizacionAPI.Models;

public class Cama
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Unidad { get; set; } = "General";
    public string Tipo { get; set; } = "Estándar";
    public string Estado { get; set; } = "Activo";  // ← Solo Estado, NO EstadoOperativo
}