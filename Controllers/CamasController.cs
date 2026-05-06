using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalizacionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CamasController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    private readonly ILogger<CamasController> _logger;

    public CamasController(HospitalizacionDbContext db, ILogger<CamasController> logger) 
    { 
        _db = db; 
        _logger = logger;
    }

    // ✅ GET /api/camas (Ruta explícita "" para evitar error 405)
    [HttpGet("")]
    public async Task<IActionResult> Listar()
    {
        try
        {
            _logger.LogInformation("📥 Listando camas...");
            var camas = await _db.Camas.Select(c => new { 
                c.Codigo, c.CodigoLogistica, c.Unidad, c.Tipo, c.EstadoOperativo, c.Estado, c.FechaRegistro 
            }).ToListAsync();
            _logger.LogInformation($"✅ {camas.Count} camas encontradas");
            return Ok(camas);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al listar camas: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    // ✅ GET /api/camas/disponibles
    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles()
    {
        try
        {
            var camas = await _db.Camas
                .Where(c => c.EstadoOperativo == "Disponible" && c.Estado == "Activo")
                .Select(c => new { c.Codigo, c.Unidad, c.Tipo })
                .ToListAsync();
            return Ok(camas);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno" });
        }
    }

    // ✅ POST /api/camas - CREAR CAMA MANUALMENTE
    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] CamaDto dto)
    {
        try
        {
            _logger.LogInformation($"📥 Intentando crear cama: {dto?.Codigo}");
            
            if (dto == null || string.IsNullOrWhiteSpace(dto.Codigo))
                return BadRequest(new { mensaje = "Datos inválidos: código requerido" });

            if (await _db.Camas.AnyAsync(c => c.Codigo == dto.Codigo))
                return BadRequest(new { mensaje = "El código de cama ya existe" });

            var cama = new Cama
            {
                Codigo = dto.Codigo,
                CodigoLogistica = dto.CodigoLogistica,
                Unidad = dto.Unidad ?? "General",
                Tipo = dto.Tipo ?? "Estándar",
                EstadoOperativo = "Disponible",
                Estado = "Activo",
                FechaRegistro = DateTime.UtcNow
            };

            _db.Camas.Add(cama);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"✅ Cama creada: {cama.Codigo}");
            
            return CreatedAtAction(nameof(Listar), new { codigo = cama.Codigo }, new { 
                mensaje = "Cama creada exitosamente", 
                cama = new { cama.Codigo, cama.Unidad, cama.Tipo } 
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError($"❌ Error de base de datos: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error al guardar en base de datos", error = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error inesperado: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    // 🔥 POST /api/camas/registrar-desde-logistica
    [HttpPost("registrar-desde-logistica")]
    public async Task<IActionResult> RegistrarDesdeLogistica([FromBody] List<CamaLogisticaDto> camasRecibidas)
    {
        try
        {
            if (camasRecibidas == null || !camasRecibidas.Any())
                return BadRequest(new { mensaje = "No se recibieron camas para registrar" });

            var creadas = new List<Cama>();
            var yaExisten = 0;

            foreach (var c in camasRecibidas)
            {
                if (await _db.Camas.AnyAsync(x => x.CodigoLogistica == c.CodigoLogistica)) { yaExisten++; continue; }
                
                string codigoUnico = "CAM-HOSP-" + Guid.NewGuid().ToString().Substring(0,using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalizacionAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CamasController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    private readonly ILogger<CamasController> _logger;

    public CamasController(HospitalizacionDbContext db, ILogger<CamasController> logger) 
    { 
        _db = db; 
        _logger = logger;
    }

    // ✅ GET /api/camas (Ruta explícita "" para evitar error 405)
    [HttpGet("")]
    public async Task<IActionResult> Listar()
    {
        try
        {
            _logger.LogInformation("📥 Listando camas...");
            var camas = await _db.Camas.Select(c => new { 
                c.Codigo, c.CodigoLogistica, c.Unidad, c.Tipo, c.EstadoOperativo, c.Estado, c.FechaRegistro 
            }).ToListAsync();
            _logger.LogInformation($"✅ {camas.Count} camas encontradas");
            return Ok(camas);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al listar camas: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
        }
    }

    // ✅ GET /api/camas/disponibles
    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles()
    {
        try
        {
            var camas = await _db.Camas
                .Where(c => c.EstadoOperativo == "Disponible" && c.Estado == "Activo")
                .Select(c => new { c.Codigo, c.Unidad, c.Tipo })
                .ToListAsync();
            return Ok(camas);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno" });
        }
    }

    // ✅ POST /api/camas - CREAR CAMA MANUALMENTE
    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] CamaDto dto)
    {
        try
        {
            _logger.LogInformation($"📥 Intentando crear cama: {dto?.Codigo}");
            
            if (dto == null || string.IsNullOrWhiteSpace(dto.Codigo))
                return BadRequest(new { mensaje = "Datos inválidos: código requerido" });

            if (await _db.Camas.AnyAsync(c => c.Codigo == dto.Codigo))
                return BadRequest(new { mensaje = "El código de cama ya existe" });

            var cama = new Cama
            {
                Codigo = dto.Codigo,
                CodigoLogistica = dto.CodigoLogistica,
                Unidad = dto.Unidad ?? "General",
                Tipo = dto.Tipo ?? "Estándar",
                EstadoOperativo = "Disponible",
                Estado = "Activo",
                FechaRegistro = DateTime.UtcNow
            };

            _db.Camas.Add(cama);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"✅ Cama creada: {cama.Codigo}");
            
            return CreatedAtAction(nameof(Listar), new { codigo = cama.Codigo }, new { 
                mensaje = "Cama creada exitosamente", 
                cama = new { cama.Codigo, cama.Unidad, cama.Tipo } 
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError($"❌ Error de base de datos: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error al guardar en base de datos", error = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error inesperado: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno", error = ex.Message });
        }
    }

    // 🔥 POST /api/camas/registrar-desde-logistica
    [HttpPost("registrar-desde-logistica")]
    public async Task<IActionResult> RegistrarDesdeLogistica([FromBody] List<CamaLogisticaDto> camasRecibidas)
    {
        try
        {
            if (camasRecibidas == null || !camasRecibidas.Any())
                return BadRequest(new { mensaje = "No se recibieron camas para registrar" });

            var creadas = new List<Cama>();
            var yaExisten = 0;

            foreach (var c in camasRecibidas)
            {
                if (await _db.Camas.AnyAsync(x => x.CodigoLogistica == c.CodigoLogistica)) { yaExisten++; continue; }
                
                string codigoUnico = "CAM-HOSP-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                creadas.Add(new Cama {
                    Codigo = codigoUnico, CodigoLogistica = c.CodigoLogistica,
                    Unidad = c.Unidad ?? "General", Tipo = c.Tipo ?? "Estándar",
                    EstadoOperativo = "Disponible", Estado = "Activo", FechaRegistro = DateTime.UtcNow
                });
            }

            if (creadas.Any())
            {
                _db.Camas.AddRange(creadas);
                await _db.SaveChangesAsync();
                return StatusCode(201, new { mensaje = $"{creadas.Count} cama(s) creadas desde Logística", nuevas = creadas.Count });
            }
            return Ok(new { mensaje = $"Todas las camas ya existían ({yaExisten})", creadas = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno" });
        }
    }

    // ✅ PUT /api/camas/{codigo}/estado
    [HttpPut("{codigo}/estado")]
    public async Task<IActionResult> CambiarEstado(string codigo, [FromBody] CambiarEstadoDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.EstadoOperativo))
                return BadRequest(new { mensaje = "estadoOperativo requerido" });

            var cama = await _db.Camas.FirstOrDefaultAsync(c => c.Codigo == codigo);
            if (cama == null) return NotFound(new { mensaje = "Cama no encontrada" });

            cama.EstadoOperativo = request.EstadoOperativo;
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = $"Estado actualizado a {request.EstadoOperativo}" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error: {ex.Message}");
            return StatusCode(500, new { mensaje = "Error interno" });
        }
    }
}

// DTOs (pueden ir en una carpeta DTOs/ o aquí)
public class CamaDto { public string? Codigo { get; set; } public string? CodigoLogistica { get; set; } public string? Unidad { get; set; } public string? Tipo { get; set; } }
public class CamaLogisticaDto { public string? CodigoLogistica { get; set; } public string? Unidad { get; set; } public string? Tipo { get; set; } }
public class CambiarEstadoDto { public string EstadoOperativo { get; set; } = ""; }