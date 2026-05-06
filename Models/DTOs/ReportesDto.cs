namespace HospitalizacionAPI.DTOs;


public class PacienteAdmisionReporteDto
{
    public string AdmisionCodigo { get; set; } = string.Empty;
    public string PacienteCodigo { get; set; } = string.Empty;
    public string PacienteNombre { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public string Especialidad { get; set; } = string.Empty;
}


public class VistaCompletaHospitalizacionDto
{
    public string AdmisionCodigo { get; set; } = string.Empty;
    public string PacienteNombre { get; set; } = string.Empty;
    public string CamaCodigo { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public string TipoCama { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public string Especialidad { get; set; } = string.Empty;
}


public class AdmisionNavegacionDto
{
    public string AdmisionCodigo { get; set; } = string.Empty;
    public string PacienteNombre { get; set; } = string.Empty;
    public string CamaCodigo { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
}