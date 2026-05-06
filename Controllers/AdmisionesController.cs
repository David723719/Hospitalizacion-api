using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;  // ← Asegúrate de tener este using

namespace HospitalizacionAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class AdmisionesController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public AdmisionesController(HospitalizacionDbContext db) => _db = db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;

namespace HospitalizacionAPI.Controllers;

[Route("api/admisiones"), ApiController]
public class AdmisionesController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public AdmisionesController(HospitalizacionDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get() => 
        Ok(await _db.Database.SqlQueryRaw<AdmisionResult>(@"SELECT ""Codigo"", ""PacienteCodigo"", ""CamaCodigo"", ""FechaIngreso"", ""Especialidad"", ""Estado"" FROM ""Admisiones""").ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateAdmisionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.Codigo)) return BadRequest(new { mensaje = "Código requerido" });
        
        // ✅ FIX: Forzar fecha a UTC
        var fechaUtc = DateTime.SpecifyKind(dto.FechaIngreso, DateTimeKind.Utc);

        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""Admisiones"" (""Codigo"", ""PacienteCodigo"", ""CamaCodigo"", ""FechaIngreso"", ""Especialidad"", ""Estado"") VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
            dto.Codigo, dto.PacienteCodigo, dto.CamaCodigo, fechaUtc, dto.Especialidad, "Activo");
        
        return Created("", new { mensaje = "Admitido", codigo = dto.Codigo });
    }
}

public class CreateAdmisionDto { public string Codigo { get; set; } = ""; public string PacienteCodigo { get; set; } = ""; public string CamaCodigo { get; set; } = ""; public DateTime FechaIngreso { get; set; } public string Especialidad { get; set; } = ""; }
public class AdmisionResult { public string Codigo { get; set; } = ""; public string PacienteCodigo { get; set; } = ""; public string CamaCodigo { get; set; } = ""; public DateTime FechaIngreso { get; set; } public string Especialidad { get; set; } = ""; public string Estado { get; set; } = ""; }
    [HttpGet("")]
    public async Task<IActionResult> Listar() => Ok(await _db.Admisiones.Select(a => new { 
        a.Codigo, a.PacienteCodigo, a.CamaCodigo, a.FechaIngreso, a.FechaEgreso, a.Especialidad, a.Estado 
    }).ToListAsync());

    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] AdmisionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.Codigo))
            return BadRequest(new { mensaje = "Código requerido" });
        if (await _db.Admisiones.AnyAsync(x => x.Codigo == dto.Codigo))
            return BadRequest(new { mensaje = "Ya existe" });

        var cama = await _db.Camas.FirstOrDefaultAsync(c =>
            c.Codigo == dto.CamaCodigo && (c.Estado == "Activo" || c.Estado == "Disponible"));
        if (cama == null) return BadRequest(new { mensaje = "Cama no disponible" });

        var paciente = await _db.Pacientes.AnyAsync(p => p.Codigo == dto.PacienteCodigo);
        if (!paciente) return BadRequest(new { mensaje = "Paciente no encontrado" });

        var admision = new Admision
        {
            Codigo = dto.Codigo,
            PacienteCodigo = dto.PacienteCodigo,
            CamaCodigo = dto.CamaCodigo,
            FechaIngreso = dto.FechaIngreso,
            FechaEgreso = dto.FechaEgreso,
            Especialidad = dto.Especialidad,
            Estado = dto.Estado ?? "Activo"
            // ❌ NO uses FechaRegistro aquí si no existe en el modelo
        };

        _db.Admisiones.Add(admision);
        cama.Estado = "Ocupada";
        await _db.SaveChangesAsync();

        return StatusCode(201, new { mensaje = "Admisión creada", admision.Codigo });
    }

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> DarAlta(string codigo)
    {
        var admision = await _db.Admisiones.FirstOrDefaultAsync(a => a.Codigo == codigo);
        if (admision == null) return NotFound(new { mensaje = "No encontrada" });

        var tratamientoActivo = await _db.Tratamientos.AnyAsync(t => t.AdmisionCodigo == codigo && t.Estado == "Activo");
        if (tratamientoActivo) return BadRequest(new { mensaje = "Hay tratamientos activos" });

        admision.Estado = "Inactivo";
        admision.FechaEgreso = DateTime.UtcNow;
        
        var cama = await _db.Camas.FirstOrDefaultAsync(c => c.Codigo == admision.CamaCodigo);
        if (cama != null) cama.Estado = "Activo";
        
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class AdmisionDto { 
    public string Codigo { get; set; } = ""; 
    public string PacienteCodigo { get; set; } = ""; 
    public string CamaCodigo { get; set; } = ""; 
    public DateTime FechaIngreso { get; set; } 
    public DateTime? FechaEgreso { get; set; } 
    public string Especialidad { get; set; } = ""; 
    public string? Estado { get; set; } 
}