namespace HospitalizacionAPI.DTOs;

public class AdmisionDto
{
    public string Codigo { get; set; } = string.Empty;
    public string PacienteCodigo { get; set; } = string.Empty;
    public string CamaCodigo { get; set; } = string.Empty;
    
    
    public DateTime FechaIngreso { get; set; }
    public DateTime? FechaEgreso { get; set; } 
    
    public string Especialidad { get; set; } = string.Empty;
    public string? Estado { get; set; }
}