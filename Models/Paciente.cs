namespace HospitalizacionAPI.Models;

public class Paciente : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
}