using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;

namespace HospitalizacionAPI.Controllers;

[ApiController]
[Route("api/mis")]
public class MISController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public MISController(HospitalizacionDbContext db) => _db = db;

    // ==========================================
    // UC1: Listado Ocupación (JOIN) + Listado General
    // ==========================================
    [HttpGet("ocupacion-camas")]
    public async Task<IActionResult> GetOcupacionCamas()
    {
        var query = from a in _db.Admisiones
                    join c in _db.Camas on a.CamaCodigo equals c.Codigo
                    join p in _db.Pacientes on a.PacienteCodigo equals p.Codigo
                    where a.Estado == "Activo"
                    select new 
                    {
                        Admision = a.Codigo,
                        Paciente = p.Nombre,
                        Cama = c.Codigo,
                        Unidad = c.Unidad,
                        Especialidad = a.Especialidad
                    };
        return Ok(await query.ToListAsync());
    }

    // ==========================================
    // UC2: Conteo Pacientes x Unidad (GROUP BY + COUNT)
    // ==========================================
    [HttpGet("conteo-unidad")]
    public async Task<IActionResult> GetConteoPorUnidad()
    {
        var query = from a in _db.Admisiones
                    join c in _db.Camas on a.CamaCodigo equals c.Codigo
                    where a.Estado == "Activo"
                    group c by c.Unidad into g
                    select new { Unidad = g.Key, TotalPacientes = g.Count() };
        return Ok(await query.ToListAsync());
    }

    // ==========================================
    // UC3: Suma Días de Estancia (GROUP BY + SUM)
    // ==========================================
    [HttpGet("suma-dias-estancia")]
    public async Task<IActionResult> GetSumaDiasEstancia()
    {
        var hoy = DateTime.UtcNow;
        var query = from a in _db.Admisiones
                    join c in _db.Camas on a.CamaCodigo equals c.Codigo
                    where a.Estado == "Activo"
                    group a by c.Unidad into g
                    select new 
                    {
                        Unidad = g.Key,
                        TotalDias = g.Sum(x => (int)(hoy - x.FechaIngreso).TotalDays)
                    };
        return Ok(await query.ToListAsync());
    }

    // ==========================================
    // UC4: Buscar por Código (Filtro)
    // ==========================================
    [HttpGet("buscar/{codigo}")]
    public async Task<IActionResult> BuscarPorCodigo(string codigo)
    {
        var query = from a in _db.Admisiones
                    join p in _db.Pacientes on a.PacienteCodigo equals p.Codigo
                    where a.Codigo == codigo
                    select new 
                    {
                        Codigo = a.Codigo,
                        Paciente = p.Nombre,
                        FechaIngreso = a.FechaIngreso,
                        Estado = a.Estado
                    };
        var resultado = await query.FirstOrDefaultAsync();
        return resultado == null ? NotFound() : Ok(resultado);
    }

    // ==========================================
    // UC5: Pacientes sin Cama (NOT EXISTS)
    // ==========================================
    [HttpGet("pacientes-sin-cama")]
    public async Task<IActionResult> GetPacientesSinCama()
    {
        var query = from p in _db.Pacientes
                    where !_db.Admisiones.Any(a => a.PacienteCodigo == p.Codigo && a.Estado == "Activo")
                    select new { Codigo = p.Codigo, Nombre = p.Nombre, Estado = p.Estado };
        return Ok(await query.ToListAsync());
    }

    // ==========================================
    // UC6: Tratamientos Activos Vigentes
    // ==========================================
    [HttpGet("tratamientos-vigentes")]
    public async Task<IActionResult> GetTratamientosVigentes()
    {
        var query = from t in _db.Tratamientos
                    join a in _db.Admisiones on t.AdmisionCodigo equals a.Codigo
                    join p in _db.Pacientes on a.PacienteCodigo equals p.Codigo
                    where t.Estado == "Activo" && a.Estado == "Activo"
                    select new 
                    {
                        Paciente = p.Nombre,
                        Medicamento = t.NombreMedicamento,
                        DiasRestantes = t.DuracionDias - (int)(DateTime.UtcNow - t.FechaInicio).TotalDays
                    };
        return Ok(await query.ToListAsync());
    }

    // ==========================================
    // UC7: Disponibilidad Camas x Especialidad
    // ==========================================
    [HttpGet("camas-disponibles")]
    public async Task<IActionResult> GetCamasDisponibles()
    {
        var query = from c in _db.Camas
                    where c.Estado == "Activo" && 
                          !_db.Admisiones.Any(a => a.CamaCodigo == c.Codigo && a.Estado == "Activo")
                    group c by c.Unidad into g
                    select new { Unidad = g.Key, CamasLibres = g.Count() };
        return Ok(await query.ToListAsync());
    }

    // ==========================================
    // UC8: Estadística Ingresos Mes
    // ==========================================
    [HttpGet("estadistica-mensual")]
    public async Task<IActionResult> GetEstadisticaMensual()
    {
        var query = from a in _db.Admisiones
                    group a by new { Mes = a.FechaIngreso.Month, Año = a.FechaIngreso.Year } into g
                    select new 
                    {
                        Periodo = $"{g.Key.Mes}/{g.Key.Año}",
                        TotalIngresos = g.Count(),
                        AltasRealizadas = g.Count(x => x.Estado == "Inactivo")
                    };
        return Ok(await query.ToListAsync());
    }

    // ==========================================
    // UC9: Reporte Medicamentos Vencidos (Auto-Check)
    // ==========================================
    [HttpGet("medicamentos-vencidos")]
    public async Task<IActionResult> GetMedicamentosVencidos()
    {
        var hoy = DateTime.UtcNow;
        var query = from t in _db.Tratamientos
                    join a in _db.Admisiones on t.AdmisionCodigo equals a.Codigo
                    where t.Estado == "Activo" && t.FechaInicio.AddDays(t.DuracionDias) <= hoy
                    select new 
                    {
                        Admision = a.Codigo,
                        Medicamento = t.NombreMedicamento,
                        FechaVencimiento = t.FechaInicio.AddDays(t.DuracionDias)
                    };
        return Ok(await query.ToListAsync());
    }

    // ==========================================
    // UC10: Eficiencia Rotación Camas
    // ==========================================
    [HttpGet("rotacion-camas")]
    public async Task<IActionResult> GetRotacionCamas()
    {
        var query = from c in _db.Camas
                    join a in _db.Admisiones on c.Codigo equals a.CamaCodigo into admGroup
                    where c.Estado == "Activo"
                    select new 
                    {
                        Cama = c.Codigo,
                        Unidad = c.Unidad,
                        VecesOcupada = admGroup.Count()
                    };
        return Ok(await query.ToListAsync());
    }
}