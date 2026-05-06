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

    // ✅ GET /api/camas
    [HttpGet("")]
    public async Task<IActionResult> Listar()
    {
        try
        {
            var camas = await _db.Camas
                .Select(c => new { c.Codigo, c.Unidad, c.Tipo, c.EstadoOperativo, c.Estado })
                .ToListAsync();
            return Ok(camas);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error listar camas: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    // ✅ GET /api/camas/disponibles
    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles()
    {
        try
        {
            var camas = await _db.Camas
                .Where(c => c.EstadoOperativo == "Disponible" && c.Estado == "Activo")
                .Select(c => new { c.Codigo, c.Unidad, c.Tipo })
                .ToListAsync();
            return Ok(camas);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error disponibles: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno" });
        }
    }

    // ✅ POST /api/camas - CREAR CAMA
    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] CamaDto dto)
    {
        try
        {
            Console.WriteLine($"📥 POST /camas: {dto?.Codigo}");
            
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
            Console.WriteLine($"✅ Cama creada: {cama.Codigo}");
            
            return CreatedAtAction(nameof(Listar), new { codigo = cama.Codigo }, new { 
                mensaje = "Cama creada", 
                cama = new { cama.Codigo, cama.Unidad, cama.Tipo, cama.EstadoOperativo } 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error crear cama: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    // ✅ PUT /api/camas/{codigo}/estado
    [HttpPut("{codigo}/estado")]
    public async Task<IActionResult> CambiarEstado(string codigo, [FromBody] CambiarEstadoDto req)
    {
        try
        {
            var cama = await _db.Camas.FirstOrDefaultAsync(c => c.Codigo == codigo);
            if (cama == null) return NotFound(new { mensaje = "Cama no encontrada" });
            
            cama.EstadoOperativo = req.EstadoOperativo;
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "Estado actualizado", nuevoEstado = req.EstadoOperativo });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error cambiar estado: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno" });
        }
    }
}

// DTOs al final del archivo
public class CamaDto { public string? Codigo { get; set; } public string? Unidad { get; set; } public string? Tipo { get; set; } }
public class CambiarEstadoDto { public string EstadoOperativo { get; set; } = ""; }