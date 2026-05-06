using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using HospitalizacionAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// 🌐 Solo escuchar en puerto dinámico (Railway lo asigna)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 🔒 CORS SEGURO: Diferente para desarrollo y producción
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // ✅ Desarrollo: permite todo (localhost, red local)
            policy.SetIsOriginAllowed(origin =>
                origin.StartsWith("http://localhost") ||
                origin.StartsWith("http://127.0.0.1") ||
                origin.StartsWith("http://10.77.") ||
                origin.StartsWith("http://192.168.") ||
                origin.StartsWith("http://172."))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }
        else
        {
            // 🔒 Producción: Solo orígenes específicos
            var allowedOrigins = builder.Configuration
                .GetSection("Security:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();
            
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// 📦 HttpClient + Configuración Externa
builder.Services.AddHttpClient();
builder.Services.Configure<ExternalServicesConfig>(builder.Configuration.GetSection("ExternalServices"));
builder.Services.AddScoped<ExternalApiService>();

// 🗄️ Base de datos (usa variable de entorno en Railway)
builder.Services.AddDbContext<HospitalizacionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Hospitalización API", 
        Version = "v1",
        Description = "API para gestión de admisiones y camas"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // 🔒 En producción: Swagger protegido o deshabilitado
    // Opcional: app.UseSwagger(); si quieres que sea público
}

app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine($"✅ Backend listo en puerto {port}");
Console.WriteLine($"✅ Environment: {app.Environment.EnvironmentName}");

app.Run();