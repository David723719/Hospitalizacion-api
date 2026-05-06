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
        Ok(await _db.Pacientes.IgnoreQueryFilters().Select(p => new { p.Codigo, p.Nombre, p.FechaNacimiento, p.Estado }).ToListAsync());

    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] PacienteDto dto)
    {
        Console.WriteLine($"📥 POST /pacientes: {dto?.Codigo}");
        
        if (string.IsNullOrWhiteSpace(dto?.Codigo))
            return BadRequest(new { mensaje = "Código requerido" });
        
        // Ignorar filtros para verificar si existe (incluye inactivos)
        if (await _db.Pacientes.IgnoreQueryFilters().AnyAsync(p => p.Codigo == dto.Codigo))
            return BadRequest(new { mensaje = "El código ya existe" });
        
        var paciente = new Paciente 
        { 
            Codigo = dto.Codigo, 
            Nombre = dto.Nombre ?? "Sin nombre", 
            FechaNacimiento = dto.FechaNacimiento,
            Estado = "Activo"
        };
        
        _db.Pacientes.Add(paciente);
        await _db.SaveChangesAsync();
        Console.WriteLine($"✅ Paciente creado: {paciente.Codigo}");
        
        return CreatedAtAction(nameof(Listar), new { codigo = paciente.Codigo }, new { 
            mensaje = "Paciente creado", 
            paciente = new { paciente.Codigo, paciente.Nombre } 
        });
    }
}

public class PacienteDto { public string Codigo { get; set; } = ""; public string? Nombre { get; set; } public DateTime FechaNacimiento { get; set; } }