using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Puerto
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// CORS abierto para evitar bloqueos desde Vercel y localhost.
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", p => p
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Base de datos - Parseo DIRECTO de tu DATABASE_URL
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
        SslMode = SslMode.Require
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

app.UseRouting();
app.UseCors("FrontendPolicy");
app.Use(async (context, next) =>
{
    context.Response.Headers["Access-Control-Allow-Origin"] = "*";
    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
    context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
    if (HttpMethods.IsOptions(context.Request.Method))
    {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }
    await next();
});
app.UseAuthorization();
app.MapControllers();

// 🔥 ENDPOINT DE PRUEBA: Siempre responde para verificar CORS/POST
app.MapPost("/api/test", () => Results.Ok(new { mensaje = "POST funciona!", timestamp = DateTime.UtcNow }));
app.MapGet("/api/test", () => Results.Ok(new { mensaje = "GET funciona!", timestamp = DateTime.UtcNow }));

Console.WriteLine($"🚀 Backend ready on port {port}");
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<HospitalizacionDbContext>();
        await db.Database.MigrateAsync();  // ← Aplica migraciones pendientes
        Console.WriteLine("✅ Migraciones aplicadas");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error en migraciones: {ex.Message}");
        // No lanzar excepción para no detener el deploy
    }
}
app.Run();