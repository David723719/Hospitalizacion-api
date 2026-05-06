
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            var o = origin.ToLowerInvariant();
           
            return o.Contains("localhost:5173") ||  // ← Frontend Vite local (CRÍTICO)
                   o.Contains("localhost:5200") ||  // ← Backend local
                   o.Contains("127.0.0.1") ||
                   o.Contains("vercel.app") ||
                   o.Contains("railway.app") ||
                   o.Contains("onrender.com") ||
                   o.Contains("ngrok") ||
                   o.StartsWith("http://10.77.") ||
                   o.StartsWith("http://192.168.") ||
                   o.StartsWith("http://172.");
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});