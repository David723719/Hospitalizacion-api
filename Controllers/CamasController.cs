using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Data;
using HospitalizacionAPI.Models;  // ← ESTE USING FALTABA - CRÍTICO
using System.Data;
using System.Data.Common;

namespace HospitalizacionAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class CamasController : ControllerBase
{
    private readonly HospitalizacionDbContext _db;
    public CamasController(HospitalizacionDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Listar()
    {
        var cols = await GetCamasColumnsAsync();
        var estadoCol = cols.Contains("EstadoOperativo") ? "\"EstadoOperativo\"" : "\"Estado\"";

        var query = $@"
            SELECT ""Codigo"", ""Unidad"", ""Tipo"", COALESCE({estadoCol}, 'Disponible') AS ""EstadoOperativo""
            FROM ""Camas""
            ORDER BY ""Codigo""";

        var rows = await QueryCamasAsync(query);
        return Ok(rows);
    }

    [HttpGet("disponibles")]
    public async Task<IActionResult> Disponibles()
    {
        var cols = await GetCamasColumnsAsync();
        var estadoCol = cols.Contains("EstadoOperativo") ? "\"EstadoOperativo\"" : "\"Estado\"";

        var query = $@"
            SELECT ""Codigo"", ""Unidad"", ""Tipo"", COALESCE({estadoCol}, 'Disponible') AS ""EstadoOperativo""
            FROM ""Camas""
            WHERE COALESCE({estadoCol}, 'Disponible') = 'Disponible'
            ORDER BY ""Codigo""";

        var rows = await QueryCamasAsync(query);
        return Ok(rows.Select(r => new { r.Codigo, r.Unidad }));
    }

    [HttpPost("")]
    public async Task<IActionResult> Crear([FromBody] CamaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.Codigo))
            return BadRequest(new { mensaje = "Código requerido" });

        var cols = await GetCamasColumnsAsync();
        var estadoCol = cols.Contains("EstadoOperativo") ? "EstadoOperativo" : "Estado";
        var hasCodigoLogistica = cols.Contains("CodigoLogistica");
        var hasFechaRegistro = cols.Contains("FechaRegistro");

        // Validar duplicado por SQL directo para evitar errores por modelo EF desalineado.
        await using var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using (var existsCmd = conn.CreateCommand())
        {
            existsCmd.CommandText = "SELECT 1 FROM \"Camas\" WHERE \"Codigo\" = @codigo LIMIT 1";
            var p = existsCmd.CreateParameter();
            p.ParameterName = "@codigo";
            p.Value = dto.Codigo!;
            existsCmd.Parameters.Add(p);

            var exists = await existsCmd.ExecuteScalarAsync();
            if (exists != null)
                return BadRequest(new { mensaje = "Ya existe" });
        }

        var columns = new List<string> { "\"Id\"", "\"Codigo\"", "\"Unidad\"", "\"Tipo\"", $"\"{estadoCol}\"" };
        var values = new List<string> { "@id", "@codigo", "@unidad", "@tipo", "@estado" };
        if (hasCodigoLogistica)
        {
            columns.Add("\"CodigoLogistica\"");
            values.Add("@codigoLogistica");
        }
        if (hasFechaRegistro)
        {
            columns.Add("\"FechaRegistro\"");
            values.Add("CURRENT_TIMESTAMP");
        }

        var insertSql = $"INSERT INTO \"Camas\" ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
        await using var insertCmd = conn.CreateCommand();
        insertCmd.CommandText = insertSql;
        AddParam(insertCmd, "@id", Guid.NewGuid());
        AddParam(insertCmd, "@codigo", dto.Codigo!);
        AddParam(insertCmd, "@unidad", dto.Unidad ?? "General");
        AddParam(insertCmd, "@tipo", dto.Tipo ?? "Estándar");
        AddParam(insertCmd, "@estado", "Disponible");
        if (hasCodigoLogistica)
            AddParam(insertCmd, "@codigoLogistica", dto.Codigo!);

        await insertCmd.ExecuteNonQueryAsync();
        return CreatedAtAction(nameof(Listar), new { codigo = dto.Codigo }, new { mensaje = "Creada", Codigo = dto.Codigo });
    }

    [HttpPut("{codigo}/estado")]
    public async Task<IActionResult> CambiarEstado(string codigo, [FromBody] CambiarEstadoDto req)
    {
        var cols = await GetCamasColumnsAsync();
        var estadoCol = cols.Contains("EstadoOperativo") ? "EstadoOperativo" : "Estado";

        await using var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"UPDATE \"Camas\" SET \"{estadoCol}\" = @estado WHERE \"Codigo\" = @codigo";
        AddParam(cmd, "@estado", req.EstadoOperativo);
        AddParam(cmd, "@codigo", codigo);
        var updated = await cmd.ExecuteNonQueryAsync();
        if (updated == 0) return NotFound(new { mensaje = "No encontrada" });

        return Ok(new { mensaje = "Actualizado" });
    }

    private async Task<HashSet<string>> GetCamasColumnsAsync()
    {
        await using var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = 'Camas'";

        var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            cols.Add(reader.GetString(0));
        return cols;
    }

    private async Task<List<CamaRow>> QueryCamasAsync(string sql)
    {
        await using var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var rows = new List<CamaRow>();

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(new CamaRow(
                reader["Codigo"]?.ToString() ?? "",
                reader["Unidad"]?.ToString() ?? "",
                reader["Tipo"]?.ToString() ?? "",
                reader["EstadoOperativo"]?.ToString() ?? "Disponible"
            ));
        }
        return rows;
    }

    private static void AddParam(DbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}

public class CamaDto { public string? Codigo { get; set; } public string? Unidad { get; set; } public string? Tipo { get; set; } }
public class CambiarEstadoDto { public string EstadoOperativo { get; set; } = ""; }
public record CamaRow(string Codigo, string Unidad, string Tipo, string EstadoOperativo);