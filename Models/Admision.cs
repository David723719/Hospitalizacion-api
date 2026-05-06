namespace HospitalizacionAPI.Models;

public class Admision : BaseEntity
{
    public string PacienteCodigo { get; set; } = string.Empty;
    public string CamaCodigo { get; set; } = string.Empty;
    
    
    public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEgreso { get; set; } // Nullable para permitir planificación futura
    
    public string Especialidad { get; set; } = string.Empty;

    // Navegación para JOINs
    public Paciente? Paciente { get; set; }
    public Cama? Cama { get; set; }
}