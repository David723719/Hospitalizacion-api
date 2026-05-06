using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.DTOs;
using HospitalizacionAPI.Models;

namespace HospitalizacionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public PacientesController(HospitalizacionDbContext db) => _db = db;

    // ✅ GET /api/pacientes (Ruta explícita para evitar conflicto 405)
    [HttpGet("")]
    public async Task<IActionResult> Get() => 
        Ok(await _db.Pacientes.Select(p => new PacienteDto { 
            Codigo = p.Codigo, Nombre = p.Nombre, FechaNacimiento = p.FechaNacimiento, Estado = p.Estado
        }).ToListAsync());

    [HttpGet("lista")]
    public async Task<IActionResult> GetAllWithInactive() => 
        Ok(await _db.Pacientes.IgnoreQueryFilters().Select(p => new PacienteDto { 
            Codigo = p.Codigo, Nombre = p.Nombre, FechaNacimiento = p.FechaNacimiento, Estado = p.Estado 
        }).ToListAsync());

    [HttpGet("buscar/{codigo}")]
    public async Task<IActionResult> GetByCode(string codigo)
    {
        var p = await _db.Pacientes.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Codigo == codigo);
        return p == null ? NotFound(new { mensaje = "Paciente no encontrado" }) : Ok(new PacienteDto { Codigo = p.Codigo, Nombre = p.Nombre, FechaNacimiento = p.FechaNacimiento });
    }

    [HttpPost("")]
    public async Task<IActionResult> Post([FromBody] PacienteDto dto)
    {
        if (await _db.Pacientes.AnyAsync(x => x.Codigo == dto.Codigo)) 
            return BadRequest(new { mensaje = "El código ya existe" });
            
        _db.Pacientes.Add(new Paciente { 
            Codigo = dto.Codigo, Nombre = dto.Nombre, 
            FechaNacimiento = dto.FechaNacimiento, Estado = "Activo", FechaRegistro = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return StatusCode(201, new { mensaje = "Creado con éxito" });
    }

    [HttpPut("{codigo}")]
    public async Task<IActionResult> Put(string codigo, [FromBody] PacienteDto dto)
    {
        var p = await _db.Pacientes.FirstOrDefaultAsync(x => x.Codigo == codigo);
        if (p == null) return NotFound(new { mensaje = "No encontrado" });
        
        p.Nombre = dto.Nombre;
        p.FechaNacimiento = dto.FechaNacimiento;
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Actualizado" });
    }

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Delete(string codigo)
    {
        var p = await _db.Pacientes.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Codigo == codigo);
        if (p == null) return NotFound(new { mensaje = "No encontrado" });
        
        p.Estado = "Inactivo";
        await _db.SaveChangesAsync();
        return NoContent();
    }
}