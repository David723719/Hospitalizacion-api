namespace HospitalizacionAPI.Models;

public class Paciente
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Estado { get; set; } = "Activo";
}