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
    public async Task<IActionResult> Listar() => 
        Ok(await _db.Pacientes.Select(p => new { p.Codigo, p.Nombre, p.FechaNacimiento, p.Estado }).ToListAsync());

    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] PacienteDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto?.Codigo))
                return BadRequest(new { mensaje = "Código requerido" });
            if (await _db.Pacientes.AnyAsync(p => p.Codigo == dto.Codigo))
                return BadRequest(new { mensaje = "Ya existe" });
            
            var paciente = new Paciente { 
                Codigo = dto.Codigo, 
                Nombre = dto.Nombre ?? "Sin nombre", 
                FechaNacimiento = dto.FechaNacimiento,
                Estado = "Activo"
            };
            _db.Pacientes.Add(paciente);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Listar), new { codigo = paciente.Codigo }, new { mensaje = "Paciente creado", paciente.Codigo });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error crear paciente: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno" });
        }
    }

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Eliminar(string codigo)
    {
        try
        {
            var paciente = await _db.Pacientes.FirstOrDefaultAsync(p => p.Codigo == codigo);
            if (paciente == null) return NotFound(new { mensaje = "No encontrado" });
            paciente.Estado = "Inactivo";
            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error eliminar: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno" });
        }
    }
}

public class PacienteDto { public string Codigo { get; set; } = ""; public string? Nombre { get; set; } public DateTime FechaNacimiento { get; set; } }