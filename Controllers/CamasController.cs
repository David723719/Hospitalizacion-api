using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalizacionAPI.Controllers;

[Route("api/camas"), ApiController]
public class CamasController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public CamasController(HospitalizacionDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try 
        {
            var camas = await _db.Database.SqlQueryRaw<CamaResult>(
                @"SELECT ""Codigo"", ""Unidad"", ""Tipo"", ""Estado"" FROM ""Camas""").ToListAsync();
            return Ok(camas);
        }
        catch (System.Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles()
    {
        try 
        {
            var camas = await _db.Database.SqlQueryRaw<CamaResult>(
                @"SELECT ""Codigo"", ""Unidad"", ""Tipo"" FROM ""Camas"" WHERE ""Estado"" = 'Activo'").ToListAsync();
            return Ok(camas);
        }
        catch (System.Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateCamaDto dto)
    {
        try 
        {
            if (string.IsNullOrWhiteSpace(dto?.Codigo)) return BadRequest(new { mensaje = "Código requerido" });
            var existe = await _db.Database.SqlQueryRaw<string>(
                @"SELECT ""Codigo"" FROM ""Camas"" WHERE ""Codigo"" = @p0", dto.Codigo).FirstOrDefaultAsync();
            if (existe != null) return BadRequest(new { mensaje = "Ya existe" });
            
            await _db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO ""Camas"" (""Codigo"", ""Unidad"", ""Tipo"", ""Estado"") VALUES (@p0, @p1, @p2, @p3)",
                dto.Codigo, dto.Unidad ?? "General", dto.Tipo ?? "Estándar", "Activo");
            
            return Created("", new { mensaje = "Creada", codigo = dto.Codigo });
        }
        catch (System.Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }

    [HttpPut("{codigo}/estado")]
    public async Task<IActionResult> Put(string codigo, [FromBody] UpdateEstadoDto dto)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                @"UPDATE ""Camas"" SET ""Estado"" = @p0 WHERE ""Codigo"" = @p1", dto.Estado, codigo);
            return Ok(new { mensaje = "Actualizado" });
        }
        catch (System.Exception ex) { return StatusCode(500, new { error = ex.Message }); }
    }
}

public class CreateCamaDto { public string Codigo { get; set; } = ""; public string? Unidad { get; set; } public string? Tipo { get; set; } }
public class UpdateEstadoDto { public string Estado { get; set; } = "Activo"; }
public class CamaResult { public string Codigo { get; set; } = ""; public string Unidad { get; set; } = ""; public string Tipo { get; set; } = ""; public string Estado { get; set; } = ""; }