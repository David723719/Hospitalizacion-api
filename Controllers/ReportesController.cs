using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;

namespace HospitalizacionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public ReportesController(HospitalizacionDbContext db) => _db = db;

    [HttpGet("pacientes-admisiones")]
    public async Task<IActionResult> GetPacientesConAdmision()
    {
        var query = from a in _db.Admisiones
                    join p in _db.Pacientes on a.PacienteCodigo equals p.Codigo
                    where a.Estado == "Activo" && p.Estado == "Activo"
                    select new 
                    {
                        AdmisionCodigo = a.Codigo,
                        PacienteCodigo = p.Codigo,
                        PacienteNombre = p.Nombre,
                        FechaIngreso = a.FechaIngreso,
                        Especialidad = a.Especialidad
                    };
        return Ok(await query.ToListAsync());
    }

    [HttpGet("vista-completa")]
    public async Task<IActionResult> GetVistaCompleta()
    {
        var query = from a in _db.Admisiones
                    join p in _db.Pacientes on a.PacienteCodigo equals p.Codigo
                    join c in _db.Camas on a.CamaCodigo equals c.Codigo
                    where a.Estado == "Activo" && p.Estado == "Activo" && c.Estado == "Activo"
                    select new 
                    {
                        AdmisionCodigo = a.Codigo,
                        PacienteNombre = p.Nombre,
                        CamaCodigo = c.Codigo,
                        Unidad = c.Unidad,
                        TipoCama = c.Tipo,
                        FechaIngreso = a.FechaIngreso,
                        Especialidad = a.Especialidad
                    };
        return Ok(await query.ToListAsync());
    }

    [HttpGet("con-navegacion")]
    public async Task<IActionResult> GetWithNavigation()
    {
        var query = from a in _db.Admisiones
                    where a.Estado == "Activo"
                    select new 
                    {
                        AdmisionCodigo = a.Codigo,
                        PacienteNombre = a.Paciente.Nombre,
                        CamaCodigo = a.Cama.Codigo,
                        Unidad = a.Cama.Unidad,
                        FechaIngreso = a.FechaIngreso
                    };
        return Ok(await query.ToListAsync());
    }

    // 📅 REPORTE: Tratamientos cuyo tiempo ya se cumplió
    [HttpGet("tratamientos-completados")]
    public async Task<IActionResult> GetTratamientosCompletados()
    {
        var hoy = DateTime.UtcNow;
        var completados = await _db.Tratamientos
            .Join(_db.Admisiones, t => t.AdmisionCodigo, a => a.Codigo, (t, a) => new { t, a })
            .Join(_db.Pacientes, x => x.a.PacienteCodigo, p => p.Codigo, (x, p) => new { x.t, x.a, p })
            .Where(x => x.a.Estado == "Activo" 
                     && x.t.Estado == "Activo" 
                     && x.t.FechaInicio.AddDays(x.t.DuracionDias) <= hoy)
            .Select(x => new 
            {
                PacienteCodigo = x.p.Codigo,
                PacienteNombre = x.p.Nombre,
                Medicamento = x.t.NombreMedicamento,
                DiasAsignados = x.t.DuracionDias,
                FechaFinCalculada = x.t.FechaInicio.AddDays(x.t.DuracionDias),
                Estado = "TIEMPO CUMPLIDO - APTO PARA ALTA O SUSPENSIÓN"
            })
            .ToListAsync();

        return Ok(completados);
    }
}