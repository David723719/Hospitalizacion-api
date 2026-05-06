using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.DTOs;
using HospitalizacionAPI.Models;

namespace HospitalizacionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdmisionesController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public AdmisionesController(HospitalizacionDbContext db) => _db = db;

    // ✅ GET /api/admisiones (Ruta explícita)
    [HttpGet("")]
    public async Task<IActionResult> Get() => Ok(await _db.Admisiones.Select(a => new AdmisionDto {
        Codigo = a.Codigo, PacienteCodigo = a.PacienteCodigo, CamaCodigo = a.CamaCodigo,
        FechaIngreso = a.FechaIngreso, FechaEgreso = a.FechaEgreso, Especialidad = a.Especialidad, Estado = a.Estado
    }).ToListAsync());

    [HttpGet("lista")]
    public async Task<IActionResult> GetAllWithInactive() => Ok(await _db.Admisiones.IgnoreQueryFilters().Select(a => new AdmisionDto {
        Codigo = a.Codigo, PacienteCodigo = a.PacienteCodigo, CamaCodigo = a.CamaCodigo,
        FechaIngreso = a.FechaIngreso, FechaEgreso = a.FechaEgreso, Especialidad = a.Especialidad, Estado = a.Estado
    }).ToListAsync());

    [HttpGet("buscar/{codigo}")]
    public async Task<IActionResult> GetByCode(string codigo)
    {
        var a = await _db.Admisiones.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Codigo == codigo);
        return a == null ? NotFound(new { mensaje = "Admisión no encontrada" }) : Ok(new AdmisionDto { 
            Codigo = a.Codigo, PacienteCodigo = a.PacienteCodigo, CamaCodigo = a.CamaCodigo,
            FechaIngreso = a.FechaIngreso, FechaEgreso = a.FechaEgreso, Especialidad = a.Especialidad 
        });
    }

    [HttpPost("")]
    public async Task<IActionResult> Post([FromBody] AdmisionDto dto)
    {
        if (dto == null) return BadRequest(new { mensaje = "Cuerpo de la petición vacío" });
        if (await _db.Admisiones.AnyAsync(x => x.Codigo == dto.Codigo))
            return BadRequest(new { mensaje = "El código de admisión ya existe" });

        // Validar que la cama esté DISPONIBLE operativamente
        var cama = await _db.Camas.FirstOrDefaultAsync(c => c.Codigo == dto.CamaCodigo && c.EstadoOperativo == "Disponible" && c.Estado == "Activo");
        if (cama == null)
            return BadRequest(new { mensaje = "La cama seleccionada no está disponible o no existe." });

        var pacienteActivo = await _db.Pacientes.AnyAsync(p => p.Codigo == dto.PacienteCodigo && p.Estado == "Activo");
        if (!pacienteActivo)
            return BadRequest(new { mensaje = "El paciente no está activo." });

        var admision = new Admision
        {
            Codigo = dto.Codigo,
            PacienteCodigo = dto.PacienteCodigo,
            CamaCodigo = dto.CamaCodigo,
            FechaIngreso = dto.FechaIngreso,
            FechaEgreso = dto.FechaEgreso,
            Especialidad = dto.Especialidad,
            Estado = dto.Estado ?? "Activo",
            FechaRegistro = DateTime.UtcNow
        };

        // Opcional: Cambiar estado de la cama a Ocupada automáticamente
        cama.EstadoOperativo = "Ocupada";

        _db.Admisiones.Add(admision);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { mensaje = "Admisión creada con éxito" });
    }

    [HttpPut("{codigo}")]
    public async Task<IActionResult> Put(string codigo, [FromBody] AdmisionDto dto)
    {
        var a = await _db.Admisiones.FirstOrDefaultAsync(x => x.Codigo == codigo);
        if (a == null) return NotFound(new { mensaje = "No encontrado" });

        a.PacienteCodigo = dto.PacienteCodigo;
        a.CamaCodigo = dto.CamaCodigo;
        a.FechaIngreso = dto.FechaIngreso;
        a.FechaEgreso = dto.FechaEgreso;
        a.Especialidad = dto.Especialidad;
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = "Actualizado" });
    }

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Delete(string codigo)
    {
        var admision = await _db.Admisiones.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Codigo == codigo);

        if (admision == null) return NotFound(new { mensaje = "Admisión no encontrada" });

        var tratamientoActivo = await _db.Tratamientos.AnyAsync(t => 
            t.AdmisionCodigo == codigo && t.Estado == "Activo");

        if (tratamientoActivo)
            return BadRequest(new { mensaje = "NO SE PUEDE DAR DE ALTA. Tratamientos activos pendientes." });

        admision.Estado = "Inactivo";
        admision.FechaEgreso = DateTime.UtcNow; // Registrar fecha de alta
        
        // Liberar la cama
        var cama = await _db.Camas.FirstOrDefaultAsync(c => c.Codigo == admision.CamaCodigo);
        if (cama != null) cama.EstadoOperativo = "Disponible";

        await _db.SaveChangesAsync();
        return NoContent();
    }
}