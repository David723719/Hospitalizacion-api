using System.ComponentModel.DataAnnotations;

namespace HospitalizacionAPI.Models;

public class Cama
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;            // Tu código interno
    public string CodigoLogistica { get; set; } = string.Empty;   // ID de Logística
    public string Unidad { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string EstadoOperativo { get; set; } = "Disponible";   // Disponible | Ocupada | Mantenimiento
    public string Estado { get; set; } = "Activo";                // Activo | Inactivo (Soft Delete)
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}