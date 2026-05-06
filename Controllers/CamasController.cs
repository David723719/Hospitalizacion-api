using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalizacionAPI.Controllers;

[Route("api/camas")]
[ApiController]
public class CamasController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public CamasController(HospitalizacionDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // ✅ SQL EXACTO a tu tabla real. EF no genera nada.
            var camas = await _db.Database.SqlQueryRaw<CamaDto>(
                @"SELECT ""Codigo"", ""Unidad"", ""Tipo"", ""Estado"" FROM ""Camas"""
            ).ToListAsync();
            return Ok(camas);
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles()
    {
        try
        {
            var camas = await _db.Database.SqlQueryRaw<CamaDto>(
                @"SELECT ""Codigo"", ""Unidad"", ""Tipo"" FROM ""Camas"" WHERE ""Estado"" = 'Activo'"
            ).ToListAsync();
            return Ok(camas);
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateCamaRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Codigo))
                return BadRequest(new { mensaje = "Código obligatorio" });

            // Verificar existencia con SQL directo
            var existe = await _db.Database.SqlQueryRaw<string>(
                @"SELECT ""Codigo"" FROM ""Camas"" WHERE ""Codigo"" = @p0", request.Codigo
            ).FirstOrDefaultAsync();

            if (existe != null) return BadRequest(new { mensaje = "Ya existe" });

            // INSERT EXACTO a columnas reales
            await _db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO ""Camas"" (""Codigo"", ""Unidad"", ""Tipo"", ""Estado"") 
                  VALUES (@p0, @p1, @p2, @p3)",
                request.Codigo, 
                request.Unidad ?? "General", 
                request.Tipo ?? "Estándar", 
                "Activo"
            );

            return Created("", new { mensaje = "Creada", codigo = request.Codigo });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{codigo}/estado")]
    public async Task<IActionResult> Put(string codigo, [FromBody] UpdateEstadoRequest request)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                @"UPDATE ""Camas"" SET ""Estado"" = @p0 WHERE ""Codigo"" = @p1",
                request.Estado, codigo
            );
            return Ok(new { mensaje = "Actualizado" });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// DTOs simples que coinciden con tu tabla
public class CreateCamaRequest { public string Codigo { get; set; } = ""; public string? Unidad { get; set; } public string? Tipo { get; set; } }
public class UpdateEstadoRequest { public string Estado { get; set; } = "Activo"; }
public class CamaDto { public string Codigo { get; set; } = ""; public string Unidad { get; set; } = ""; public string Tipo { get; set; } = ""; public string Estado { get; set; } = ""; }