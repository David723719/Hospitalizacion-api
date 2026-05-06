using Microsoft.AspNetCore.Mvc;
using HospitalizacionAPI.Services;
using HospitalizacionAPI.Models;
using Microsoft.Extensions.Options;

namespace HospitalizacionAPI.Controllers;

[ApiController, Route("api/integracion")]
public class IntegracionController : ControllerBase
{
    private readonly ExternalApiService _api;
    private readonly IOptionsMonitor<ExternalServicesConfig> _cfg;

    public IntegracionController(ExternalApiService api, IOptionsMonitor<ExternalServicesConfig> cfg)
    { _api = api; _cfg = cfg; }

    [HttpGet("estado")]
    public IActionResult GetStatus()
    {
        var c = _cfg.CurrentValue;
        return Ok(new
        {
            Farmacia = new { enabled = c.Farmacia.Enabled, url = c.Farmacia.BaseUrl },
            Emergencias = new { enabled = c.Emergencias.Enabled, url = c.Emergencias.BaseUrl },
            RRHH = new { enabled = c.RecursosHumanos.Enabled, url = c.RecursosHumanos.BaseUrl },
            Facturacion = new { enabled = c.Facturacion.Enabled, url = c.Facturacion.BaseUrl },
            Logistica = new { enabled = c.Logistica.Enabled, url = c.Logistica.BaseUrl },
            Mensaje = "Configuración activa. Cambios en appsettings.json se aplican sin reiniciar."
        });
    }

    // Endpoints de prueba para tus consumos externos
    [HttpGet("farmacia/medicamentos")] public async Task<IActionResult> FarmaciaMed() => Ok(new { ok = true, data = await _api.GetMedicamentos() });
    [HttpGet("emergencias/triaje")] public async Task<IActionResult> EmergTriaje() => Ok(new { ok = true, data = await _api.GetTriaje() });
    [HttpGet("rrhh/medicos")] public async Task<IActionResult> RRHHMed([FromQuery] string? esp) => Ok(new { ok = true, data = await _api.GetMedicos(esp) });
    [HttpGet("facturacion/seguro/{cod}")] public async Task<IActionResult> FactSeg(string cod) => Ok(new { ok = true, data = await _api.ValidarSeguro(cod) });
}