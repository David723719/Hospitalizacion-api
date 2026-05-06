using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;  // ← ESTE USING FALTABA - CRÍTICO

namespace HospitalizacionAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class CamasController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public CamasController(HospitalizacionDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Listar() => 
        Ok(await _db.Camas.Select(c => new { c.Codigo, c.Unidad, c.Tipo, c.EstadoOperativo }).ToListAsync());

    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles() => 
        Ok(await _db.Camas.Where(c => c.EstadoOperativo == "Disponible").Select(c => new { c.Codigo, c.Unidad }).ToListAsync());

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
            EstadoOperativo = "Disponible",
            Estado = "Activo"
        };
        _db.Camas.Add(cama);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Listar), new { codigo = cama.Codigo }, new { mensaje = "Creada", cama.Codigo });
    }

    [HttpPut("{codigo}/estado")]
    public async Task<IActionResult> CambiarEstado(string codigo, [FromBody] CambiarEstadoDto req)
    {
        var cama = await _db.Camas.FirstOrDefaultAsync(c => c.Codigo == codigo);
        if (cama == null) return NotFound(new { mensaje = "No encontrada" });
        cama.EstadoOperativo = req.EstadoOperativo;
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Actualizado" });
    }
}

public class CamaDto { public string? Codigo { get; set; } public string? Unidad { get; set; } public string? Tipo { get; set; } }
public class CambiarEstadoDto { public string EstadoOperativo { get; set; } = ""; }