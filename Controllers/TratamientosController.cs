using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;

namespace HospitalizacionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TratamientosController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public TratamientosController(HospitalizacionDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Tratamientos.Select(t => new {
            Codigo = t.Codigo,
            AdmisionCodigo = t.AdmisionCodigo,
            NombreMedicamento = t.NombreMedicamento,
            Dosis = t.Dosis,
            DuracionDias = t.DuracionDias,
            FechaInicio = t.FechaInicio
        }).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Tratamiento tratamiento)
    {
        var admisionActiva = await _db.Admisiones.AnyAsync(a => a.Codigo == tratamiento.AdmisionCodigo && a.Estado == "Activo");
        if (!admisionActiva) return BadRequest("La admisión no existe o el paciente ya fue dado de alta");

        if (await _db.Tratamientos.AnyAsync(t => t.Codigo == tratamiento.Codigo))
            return BadRequest("El código de tratamiento ya existe");

        tratamiento.Estado = "Activo";
        tratamiento.FechaInicio = DateTime.UtcNow;

        _db.Tratamientos.Add(tratamiento);
        await _db.SaveChangesAsync();
        return StatusCode(201, new { mensaje = "Tratamiento asignado correctamente" });
    }

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Delete(string codigo)
    {
        var tratamiento = await _db.Tratamientos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Codigo == codigo);
        if (tratamiento == null) return NotFound();

        tratamiento.Estado = "Inactivo";
        await _db.SaveChangesAsync();
        return NoContent();
    }
}