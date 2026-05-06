namespace HospitalizacionAPI.Models;

public class Admision : BaseEntity  // o sin herencia si no usas BaseEntity
{
    public string PacienteCodigo { get; set; } = string.Empty;
    public string CamaCodigo { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEgreso { get; set; }
    public string Especialidad { get; set; } = string.Empty;
    
   
    public Paciente? Paciente { get; set; }
    public Cama? Cama { get; set; }
}