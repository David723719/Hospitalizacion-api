using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalizacionAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class CamasController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public CamasController(HospitalizacionDbContext db) => _db = db;

    // ✅ GET /api/camas (Ruta explícita para evitar conflicto 405)
    [HttpGet("")]
    public async Task<IActionResult> Listar() => 
        Ok(await _db.Camas.Select(c => new { 
            c.Codigo, c.Unidad, c.Tipo, c.EstadoOperativo, c.Estado, c.CodigoLogistica 
        }).ToListAsync());

    // ✅ GET /api/camas/disponibles
    [HttpGet("disponibles")]
    public async Task<IActionResult> ListarDisponibles() => 
        Ok(await _db.Camas
            .Where(c => c.EstadoOperativo == "Disponible" && c.Estado == "Activo")
            .Select(c => new { c.Codigo, c.Unidad, c.Tipo })
            .ToListAsync());

    // ✅ POST /api/camas (Para registrar manualmente desde tu frontend)
    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] CamaDto dto)
    {
        if (await _db.Camas.AnyAsync(c => c.Codigo == dto.Codigo))
            return BadRequest(new { mensaje = "El código de cama ya existe" });

        var cama = new Cama
        {
            Codigo = dto.Codigo ?? "CAM-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
            CodigoLogistica = dto.CodigoLogistica,
            Unidad = dto.Unidad ?? "General",
            Tipo = dto.Tipo ?? "Estándar",
            EstadoOperativo = "Disponible",
            Estado = "Activo",
            FechaRegistro = DateTime.UtcNow
        };
        _db.Camas.Add(cama);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Listar), new { codigo = cama.Codigo }, new { mensaje = "Cama creada", cama });
    }

    // 🔥 POST /api/camas/registrar-desde-logistica (Tu código original)
    [HttpPost("registrar-desde-logistica")]
    public async Task<IActionResult> RegistrarDesdeLogistica([FromBody] List<CamaLogisticaDto> camasRecibidas)
    {
        if (camasRecibidas == null || !camasRecibidas.Any())
            return BadRequest(new { mensaje = "No se recibieron camas para registrar" });

        var creadas = new List<Cama>();
        var yaExisten = 0;

        foreach (var c in camasRecibidas)
        {
            if (await _db.Camas.AnyAsync(x => x.CodigoLogistica == c.CodigoLogistica)) { yaExisten++; continue; }
            
            string codigoUnico = "CAM-HOSP-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            creadas.Add(new Cama {
                Codigo = codigoUnico, CodigoLogistica = c.CodigoLogistica,
                Unidad = c.Unidad, Tipo = c.Tipo,
                EstadoOperativo = "Disponible", Estado = "Activo", FechaRegistro = DateTime.UtcNow
            });
        }

        if (!creadas.Any()) return Ok(new { mensaje = $"Todas existían ({yaExisten})", creadas = 0 });
        
        _db.Camas.AddRange(creadas);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { mensaje = $"{creadas.Count} creadas", nuevas = creadas.Count, camas = creadas });
    }

    // ✅ PUT /api/camas/{codigo}/estado
    [HttpPut("{codigo}/estado")]
    public async Task<IActionResult> CambiarEstado(string codigo, [FromBody] Dictionary<string, string> request)
    {
        if (!request.TryGetValue("estadoOperativo", out var nuevoEstado))
            return BadRequest(new { mensaje = "Debe enviar 'estadoOperativo'" });

        var cama = await _db.Camas.FirstOrDefaultAsync(c => c.Codigo == codigo);
        if (cama == null) return NotFound(new { mensaje = "Cama no encontrada" });

        cama.EstadoOperativo = nuevoEstado;
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = $"Estado: {nuevoEstado}" });
    }
}
public class CamaDto { public string? Codigo { get; set; } public string? CodigoLogistica { get; set; } public string? Unidad { get; set; } public string? Tipo { get; set; } }