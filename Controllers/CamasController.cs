using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;

namespace HospitalizacionAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class CamasController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public CamasController(HospitalizacionDbContext db) => _db = db;

    // ✅ GET /api/camas - Solo columnas que existen
    [HttpGet("")]
    public async Task<IActionResult> Listar()
    {
        var camas = await _db.Camas
            .Select(c => new { c.Codigo, c.Unidad, c.Tipo, c.Estado })
            .ToListAsync();
        return Ok(camas);
    }

    // ✅ GET /api/camas/disponibles - Filtrar por Estado = 'Activo'
    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles()
    {
        var camas = await _db.Camas
            .Where(c => c.Estado == "Activo")
            .Select(c => new { c.Codigo, c.Unidad, c.Tipo })
            .ToListAsync();
        return Ok(camas);
    }

    // ✅ POST /api/camas - Crear con columnas reales
    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] CamaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.Codigo))
            return BadRequest(new { mensaje = "Código requerido" });
        
        if (await _db.Camas.AnyAsync(c => c.Codigo == dto.Codigo))
            return BadRequest(new { mensaje = "Ya existe" });
        
        var cama = new Cama { 
            Codigo = dto.Codigo, 
            Unidad = dto.Unidad ?? "General", 
            Tipo = dto.Tipo ?? "Estándar",
            Estado = "Activo"  // ← Solo Estado
        };
        
        _db.Camas.Add(cama);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Listar), new { codigo = cama.Codigo }, new { mensaje = "Creada", cama.Codigo });
    }

    // ✅ PUT /api/camas/{codigo}/estado - Cambiar Estado (Activo/Inactivo)
    [HttpPut("{codigo}/estado")]
    public async Task<IActionResult> CambiarEstado(string codigo, [FromBody] CambiarEstadoDto req)
    {
        var cama = await _db.Camas.FirstOrDefaultAsync(c => c.Codigo == codigo);
        if (cama == null) return NotFound(new { mensaje = "No encontrada" });
        
        cama.Estado = req.Estado;  // ← Cambia Estado, no EstadoOperativo
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Actualizado" });
    }
}

public class CamaDto { public string? Codigo { get; set; } public string? Unidad { get; set; } public string? Tipo { get; set; } }
public class CambiarEstadoDto { public string Estado { get; set; } = "Activo"; }  // ← Solo Estado