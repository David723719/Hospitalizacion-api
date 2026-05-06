using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;

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
}

public class CamaDto { public string? Codigo { get; set; } public string? Unidad { get; set; } public string? Tipo { get; set; } }