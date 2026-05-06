using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;

namespace HospitalizacionAPI.Controllers;

[ApiController, Route("api/sistema")]
public class SistemaController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public SistemaController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("mi-ip")]
    public IActionResult GetMiIp()
    {
        // 🔒 SOLO en desarrollo local
        if (!_env.IsDevelopment())
        {
            return Unauthorized(new { 
                mensaje = "Endpoint solo disponible en desarrollo local",
                produccionUrl = "https://tu-app.railway.app"
            });
        }

        var ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList
            .Where(i => i.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(i))
            .Select(i => i.ToString()).ToList();

        var principal = ips.FirstOrDefault(i => i.StartsWith("10.") || i.StartsWith("192.168."));

        return Ok(new
        {
            Mensaje = "IP actual (solo desarrollo)",
            IP_Principal = principal,
            Nota = "En producción usa la URL de Railway"
        });
    }

    [HttpGet("health")]
    public IActionResult Health() => Ok(new { 
        Status = "OK", 
        Servicio = "Hospitalización API",
        Environment = _env.EnvironmentName,
        Timestamp = DateTime.UtcNow 
    });
}