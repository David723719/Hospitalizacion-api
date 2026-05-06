namespace HospitalizacionAPI.DTOs;

public class PacienteDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string? Estado { get; set; }
}