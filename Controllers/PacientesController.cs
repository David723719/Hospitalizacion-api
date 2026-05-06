using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;

namespace HospitalizacionAPI.Controllers;

[Route("api/pacientes"), ApiController]
public class PacientesController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public PacientesController(HospitalizacionDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get() => 
        Ok(await _db.Database.SqlQueryRaw<PacienteResult>(@"SELECT ""Codigo"", ""Nombre"", ""FechaNacimiento"", ""Estado"" FROM ""Pacientes""").ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreatePacienteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.Codigo)) return BadRequest(new { mensaje = "Código requerido" });
        if (await _db.Database.SqlQueryRaw<string>(@"SELECT ""Codigo"" FROM ""Pacientes"" WHERE ""Codigo"" = @p0", dto.Codigo).FirstOrDefaultAsync() != null)
            return BadRequest(new { mensaje = "Ya existe" });

        var id = Guid.NewGuid();
        var fechaNac = dto.FechaNacimiento ?? DateTime.UtcNow;
        await _db.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""Pacientes"" (""Id"", ""Codigo"", ""Nombre"", ""FechaNacimiento"", ""Estado"") VALUES (@p0, @p1, @p2, @p3, @p4)",
            id, dto.Codigo, dto.Nombre ?? "Sin nombre", fechaNac, "Activo");
        
        return Created("", new { mensaje = "Creado", codigo = dto.Codigo });
    }

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Delete(string codigo)
    {
        await _db.Database.ExecuteSqlRawAsync(@"UPDATE ""Pacientes"" SET ""Estado"" = 'Inactivo' WHERE ""Codigo"" = @p0", codigo);
        return NoContent();
    }
}

public class CreatePacienteDto { public string Codigo { get; set; } = ""; public string? Nombre { get; set; } public DateTime? FechaNacimiento { get; set; } }
public class PacienteResult { public string Codigo { get; set; } = ""; public string Nombre { get; set; } = ""; public DateTime FechaNacimiento { get; set; } public string Estado { get; set; } = ""; }