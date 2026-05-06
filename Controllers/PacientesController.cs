using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;

namespace HospitalizacionAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public PacientesController(HospitalizacionDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Listar() => Ok(await _db.Pacientes.Select(p => new { p.Codigo, p.Nombre, p.FechaNacimiento, p.Estado }).ToListAsync());

    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] PacienteDto dto)
    {
        if (await _db.Pacientes.AnyAsync(p => p.Codigo == dto.Codigo))
            return BadRequest(new { mensaje = "El código ya existe" });
        var paciente = new Paciente { Codigo = dto.Codigo, Nombre = dto.Nombre, FechaNacimiento = dto.FechaNacimiento, Estado = dto.Estado ?? "Activo", FechaRegistro = DateTime.UtcNow };
        _db.Pacientes.Add(paciente);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { mensaje = "Paciente creado" });
    }

    [HttpPut("{codigo}")]
    public async Task<IActionResult> Actualizar(string codigo, [FromBody] PacienteDto dto)
    {
        var p = await _db.Pacientes.FirstOrDefaultAsync(x => x.Codigo == codigo);
        if (p == null) return NotFound(new { mensaje = "No encontrado" });
        p.Nombre = dto.Nombre; p.FechaNacimiento = dto.FechaNacimiento;
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Actualizado" });
    }

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Eliminar(string codigo)
    {
        var p = await _db.Pacientes.FirstOrDefaultAsync(x => x.Codigo == codigo);
        if (p == null) return NotFound(new { mensaje = "No encontrado" });
        p.Estado = "Inactivo";
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
public class PacienteDto { public string Codigo { get; set; } = ""; public string Nombre { get; set; } = ""; public string FechaNacimiento { get; set; } = ""; public string? Estado { get; set; } }