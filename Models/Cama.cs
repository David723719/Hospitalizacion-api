namespace HospitalizacionAPI.Models;

public class Cama
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Unidad { get; set; } = "General";
    public string Tipo { get; set; } = "Estándar";
    public string Estado { get; set; } = "Activo";  // ← Solo Estado, NO EstadoOperativo
}