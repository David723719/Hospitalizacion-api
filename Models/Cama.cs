using System.ComponentModel.DataAnnotations;

namespace HospitalizacionAPI.Models;

public class Cama
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = "Disponible"; // Disponible | Ocupada | Mantenimiento
}