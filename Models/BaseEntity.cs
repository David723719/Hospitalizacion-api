namespace HospitalizacionAPI.Models;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = string.Empty;
    public string Estado { get; set; } = "Activo";
}