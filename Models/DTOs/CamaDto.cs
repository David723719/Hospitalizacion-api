namespace HospitalizacionAPI.DTOs;

public class CamaDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Estado { get; set; }
}