// 🔒 CORS SEGURO: Funciona en desarrollo Y producción (Vercel + Railway)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        // ✅ Producción: Usa predicado flexible para permitir dominios dinámicos
        policy.SetIsOriginAllowed(origin =>
        {
            var o = origin.ToLowerInvariant();
            return 
                o.Contains("localhost") ||           // Desarrollo local
                o.Contains("127.0.0.1") ||           // Desarrollo local IP
                o.Contains("vercel.app") ||          // Frontend en Vercel ✅
                o.Contains("railway.app") ||         // Backend en Railway ✅
                o.Contains("onrender.com") ||        // Por si usas Render
                o.StartsWith("http://10.77.") ||     // Red UPDS
                o.StartsWith("http://192.168.") ||   // Red casa
                o.StartsWith("http://172.");         // Red corporativa
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});