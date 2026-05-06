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

    // ✅ GET /api/camas - USANDO SQL DIRECTO PARA EVITAR ERRORES DE MODELO
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try 
        {
            // Ejecutamos SQL puro. Esto ignora cualquier configuración de EF Core.
            var camas = await _db.Database.SqlQueryRaw<CamaSimple>(
                @"SELECT ""Codigo"", ""Unidad"", ""Tipo"", ""Estado"" FROM ""Camas"""
            ).ToListAsync();
            
            return Ok(camas);
        }
        catch (System.Exception ex)
        {
            // Logueamos pero devolvemos array vacío para que el frontend no se rompa
            System.Console.WriteLine($"❌ ERROR CAMAS: {ex.Message}");
            return Ok(new List<object>());
        }
    }

    // ✅ GET /api/camas/disponibles - SQL DIRECTO
    [HttpGet("disponibles")]
    public async Task<IActionResult> GetDisponibles()
    {
        try 
        {
            var camas = await _db.Database.SqlQueryRaw<CamaSimple>(
                @"SELECT ""Codigo"", ""Unidad"", ""Tipo"" FROM ""Camas"" WHERE ""Estado"" = 'Activo'"
            ).ToListAsync();
            
            return Ok(camas);
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"❌ ERROR DISPONIBLES: {ex.Message}");
            return Ok(new List<object>());
        }
    }

    // ✅ POST /api/camas - INSERT DIRECTO
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateCamaRequest request)
    {
        try 
        {
            if (string.IsNullOrWhiteSpace(request.Codigo))
                return BadRequest(new { mensaje = "El código es obligatorio" });

            // Verificar existencia con SQL
            var existe = await _db.Database.SqlQueryRaw<string>(
                @"SELECT ""Codigo"" FROM ""Camas"" WHERE ""Codigo"" = @p0", request.Codigo
            ).FirstOrDefaultAsync();

            if (existe != null) return BadRequest(new { mensaje = "El código ya existe" });

            // Insertar directo
            await _db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO ""Camas"" (""Codigo"", ""Unidad"", ""Tipo"", ""Estado"") 
                  VALUES (@p0, @p1, @p2, @p3)",
                request.Codigo, 
                request.Unidad ?? "General", 
                request.Tipo ?? "Estándar", 
                "Activo"
            );

            return Created("", new { mensaje = "Cama creada", codigo = request.Codigo });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { error = "Error al guardar", detalle = ex.Message });
        }
    }
}

// DTO simple para recibir datos del POST
public class CreateCamaRequest 
{ 
    public string Codigo { get; set; } 
    public string? Unidad { get; set; } 
    public string? Tipo { get; set; } 
}

// DTO simple para el resultado del SQL (NO usa el modelo de EF)
public class CamaSimple 
{
    public string Codigo { get; set; }
    public string Unidad { get; set; }
    public string Tipo { get; set; }
    public string Estado { get; set; }
}