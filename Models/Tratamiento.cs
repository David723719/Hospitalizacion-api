namespace HospitalizacionAPI.Models;

public class Tratamiento : BaseEntity
{
    public string AdmisionCodigo { get; set; } = string.Empty;
    public string NombreMedicamento { get; set; } = string.Empty;
    public string Dosis { get; set; } = string.Empty;
    public int DuracionDias { get; set; }
    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;

    public Admision? Admision { get; set; }
}