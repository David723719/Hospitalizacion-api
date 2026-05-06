namespace HospitalizacionAPI.Models;

public class Cama
{
    public string Codigo { get; set; } = string.Empty;
    public string Unidad { get; set; } = "General";
    public string Tipo { get; set; } = "Estándar";
    public string EstadoOperativo { get; set; } = "Disponible";  
    public string Estado { get; set; } = "Activo";                