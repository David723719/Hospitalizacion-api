using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using System.Collections.Generic; // Para List y Dictionary
using System.Linq;

namespace HospitalizacionAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class CamasController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;

    public CamasController(HospitalizacionDbContext db) => _db = db;

    // 🔹 GET: Ver todas tus camas
    [HttpGet]
    public async Task<IActionResult> Get() => 
        Ok(await _db.Camas.Select(c => new { 
            c.Codigo, 
            c.Unidad, 
            c.Tipo, 
            c.EstadoOperativo, 
            c.Estado, 
            c.CodigoLogistica 
        }).ToListAsync());

    // 🔹 GET: Solo disponibles para pacientes
    [HttpGet("disponibles")]
    public async Task<IActionResult> GetDisponibles() => 
        Ok(await _db.Camas
            .Where(c => c.EstadoOperativo == "Disponible" && c.Estado == "Activo")
            .Select(c => new { c.Codigo, c.Unidad, c.Tipo })
            .ToListAsync());

    // 🔥 POST: RECIBE CAMAS DESDE LOGÍSTICA Y LAS CREA EN TU TABLA
    [HttpPost("registrar-desde-logistica")]
    public async Task<IActionResult> RegistrarDesdeLogistica([FromBody] List<CamaLogisticaDto> camasRecibidas)
    {
        if (camasRecibidas == null || !camasRecibidas.Any())
            return BadRequest(new { mensaje = "No se recibieron camas para registrar" });

        var creadas = new List<Cama>();
        var yaExisten = 0;

        foreach (var c in camasRecibidas)
        {
            // Validar que no exista ya por CódigoLogística
            if (await _db.Camas.AnyAsync(x => x.CodigoLogistica == c.CodigoLogistica))
            { 
                yaExisten++; 
                continue; 
            }

            // Generar código único de forma segura
            string codigoUnico = "CAM-HOSP-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            creadas.Add(new Cama
            {
                Codigo = codigoUnico,
                CodigoLogistica = c.CodigoLogistica,
                Unidad = c.Unidad,
                Tipo = c.Tipo,
                EstadoOperativo = "Disponible",
                Estado = "Activo",
                FechaRegistro = DateTime.UtcNow
            });
        }

        if (!creadas.Any())
            return Ok(new { mensaje = $"Todas las camas ya existían ({yaExisten} encontradas)", creadas = 0 });

        // Guardar en la base de datos
        _db.Camas.AddRange(creadas);
        await _db.SaveChangesAsync();

        return StatusCode(201, new 
        { 
            mensaje = $"{creadas.Count} cama(s) creadas exitosamente",
            nuevas = creadas.Count,
            ya_existian = yaExisten,
            camas = creadas.Select(c => new { c.Codigo, c.CodigoLogistica, c.Unidad, c.Tipo })
        });
    }

    // 🔹 PUT: Cambiar estado (Disponible, Ocupada, Mantenimiento)
    [HttpPut("{codigo}/estado")]
    public async Task<IActionResult> CambiarEstado(string codigo, [FromBody] Dictionary<string, string> request)
    {
        if (!request.TryGetValue("estadoOperativo", out var nuevoEstado))
            return BadRequest(new { mensaje = "Debe enviar 'estadoOperativo' en el cuerpo." });

        var cama = await _db.Camas.FirstOrDefaultAsync(c => c.Codigo == codigo);
        if (cama == null) return NotFound(new { mensaje = "Cama no encontrada" });

        // Validación de seguridad
        if (nuevoEstado == "Disponible")
        {
            var hayPaciente = await _db.Admisiones.AnyAsync(a => a.CamaCodigo == codigo && a.Estado == "Activo");
            if (hayPaciente) 
                return BadRequest(new { mensaje = "No se puede liberar: hay una admisión activa en esta cama." });
        }

        cama.EstadoOperativo = nuevoEstado;
        await _db.SaveChangesAsync();
        return Ok(new { mensaje = $"Estado actualizado a {nuevoEstado}" });
    }
}