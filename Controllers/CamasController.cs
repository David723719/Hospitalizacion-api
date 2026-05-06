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
            Console.WriteLine("📥 GET /camas - Ejecutando SQL directo");
            
            // SQL DIRECTO - Sin LINQ, sin generación automática
            var camas = await _db.Database
                .SqlQueryRaw<CamaResult>(@"
                    SELECT ""Codigo"", ""Unidad"", ""Tipo"", 
                           COALESCE(""EstadoOperativo"", 'Disponible') as ""EstadoOperativo"",
                           COALESCE(""Estado"", 'Activo') as ""Estado""
                    FROM ""Camas""
                ")
                .ToListAsync();
            
            Console.WriteLine($"✅ Listadas {camas.Count} camas");
            return Ok(camas);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌❌❌ ERROR LISTAR CAMAS ❌❌❌");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
            }
            return StatusCode(500, new { 
                mensaje = "Error interno", 
                error = ex.Message,
                inner = ex.InnerException?.Message,
                hint = "Revisa /api/debug para ver estado de la DB"
            });
        }
    }

    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles()
    {
        try
        {
            var camas = await _db.Database
                .SqlQueryRaw<CamaResult>(@"
                    SELECT ""Codigo"", ""Unidad"", ""Tipo"",
                           COALESCE(""EstadoOperativo"", 'Disponible') as ""EstadoOperativo"",
                           COALESCE(""Estado"", 'Activo') as ""Estado""
                    FROM ""Camas""
                    WHERE COALESCE(""EstadoOperativo"", 'Disponible') = 'Disponible'
                    AND COALESCE(""Estado"", 'Activo') = 'Activo'
                ")
                .ToListAsync();
            return Ok(camas);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error disponibles: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] CamaDto dto)
    {
        try
        {
            Console.WriteLine($"📥 POST /camas: {dto?.Codigo}");
            
            if (string.IsNullOrWhiteSpace(dto?.Codigo))
                return BadRequest(new { mensaje = "Código requerido" });
            
            // Verificar existencia con SQL directo
            var existe = await _db.Database
                .SqlQueryRaw<string>(@"SELECT ""Codigo"" FROM ""Camas"" WHERE ""Codigo"" = @p0", dto.Codigo)
                .FirstOrDefaultAsync();
            
            if (existe != null)
                return BadRequest(new { mensaje = "Ya existe" });
            
            // Insertar con SQL directo
            await _db.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ""Camas"" (""Codigo"", ""Unidad"", ""Tipo"", ""EstadoOperativo"", ""Estado"")
                VALUES (@p0, @p1, @p2, @p3, @p4)
            ", dto.Codigo, dto.Unidad ?? "General", dto.Tipo ?? "Estándar", "Disponible", "Activo");
            
            Console.WriteLine($"✅ Cama creada: {dto.Codigo}");
            return CreatedAtAction(nameof(Listar), new { codigo = dto.Codigo }, new { 
                mensaje = "Creada", Codigo = dto.Codigo 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌❌❌ ERROR CREAR CAMA ❌❌❌");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            return StatusCode(500, new { 
                mensaje = "Error interno", 
                error = ex.Message,
                hint = "Revisa /api/debug para ver columnas de la tabla Camas"
            });
        }
    }
}

// Resultado simple para SQL Query
public class CamaResult { 
    public string Codigo { get; set; } = ""; 
    public string Unidad { get; set; } = ""; 
    public string Tipo { get; set; } = ""; 
    public string EstadoOperativo { get; set; } = "Disponible"; 
    public string Estado { get; set; } = "Activo"; 
}

public class CamaDto { public string? Codigo { get; set; } public string? Unidad { get; set; } public string? Tipo { get; set; } }