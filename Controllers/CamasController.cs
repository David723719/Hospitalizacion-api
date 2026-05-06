using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;

namespace HospitalizacionAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class CamasController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public CamasController(HospitalizacionDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Listar()
    {
        try
        {
            // Usamos SQL directo para evitar errores de Entity Framework si el modelo no coincide exactamente
            var camas = await _db.Database
                .SqlQueryRaw<CamaSimple>(@"
                    SELECT ""Codigo"", ""Unidad"", ""Tipo"", 
                           COALESCE(""EstadoOperativo"", 'Disponible') AS ""EstadoOperativo""
                    FROM ""Camas""
                ")
                .ToListAsync();
            
            return Ok(camas);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en Listar Camas: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles()
    {
        try
        {
            var camas = await _db.Database
                .SqlQueryRaw<CamaSimple>(@"
                    SELECT ""Codigo"", ""Unidad"", ""Tipo"", 
                           COALESCE(""EstadoOperativo"", 'Disponible') AS ""EstadoOperativo""
                    FROM ""Camas""
                    WHERE COALESCE(""EstadoOperativo"", 'Disponible') = 'Disponible'
                ")
                .ToListAsync();
            
            return Ok(camas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al obtener disponibles", error = ex.Message });
        }
    }

    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] CamaDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto?.Codigo))
                return BadRequest(new { mensaje = "Código requerido" });

            // Insertar directamente
            await _db.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Camas"" (""Codigo"", ""Unidad"", ""Tipo"", ""EstadoOperativo"")
                VALUES (@p0, @p1, @p2, @p3)
            ", dto.Codigo, dto.Unidad ?? "General", dto.Tipo ?? "Estándar", "Disponible");

            return CreatedAtAction(nameof(Listar), new { codigo = dto.Codigo }, new { mensaje = "Cama creada" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al crear", error = ex.Message });
        }
    }
}

// Clase simple para mapear el resultado del SQL
public class CamaSimple 
{ 
    public string Codigo { get; set; } = ""; 
    public string Unidad { get; set; } = ""; 
    public string Tipo { get; set; } = ""; 
    public string EstadoOperativo { get; set; } = "Disponible"; 
}

public class CamaDto { public string? Codigo { get; set; } public string? Unidad { get; set; } public string? Tipo { get; set; } }