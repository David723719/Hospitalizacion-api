using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 🌐 Puerto dinámico
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔒 CORS: PERMITE TODO (para desarrollo - ajustar en producción)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // ← Permite cualquier origen (desarrollo)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 🗄️ Base de datos CON MANEJO DE ERRORES
try
{
    var connectionString = ResolveConnectionString(builder.Configuration);
    builder.Services.AddDbContext<HospitalizacionDbContext>(options =>
        options.UseNpgsql(connectionString));
    Console.WriteLine($"✅ DB configurada");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERROR DB: {ex.Message}");
    // No lanzar excepción para que el backend arranque igual
}

// 📦 Controllers
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hospital API", Version = "v1" });
});

var app = builder.Build();

// 🔄 Middleware con logging
app.Use(async (context, next) =>
{
    Console.WriteLine($"📥 {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"📤 {context.Response.StatusCode}");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "API"); c.RoutePrefix = "swagger"; });
}

app.UseCors("AllowAll"); // ← Nombre debe coincidir
app.UseAuthorization();
app.MapControllers();

Console.WriteLine($"🚀 Backend listo en puerto {port}");
app.Run();

// 🔧 Resolver conexión
static string ResolveConnectionString(IConfiguration config)
{
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(dbUrl))
    {
        var uri = new Uri(dbUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host, Port = uri.Port,
            Username = Uri.UnescapeDataString(userInfo.Length > 0 ? userInfo[0] : ""),
            Password = Uri.UnescapeDataString(userInfo.Length > 1 ? userInfo[1] : ""),
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode = SslMode.Require
        }.ConnectionString;
    }
    return config.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("No connection string");
}