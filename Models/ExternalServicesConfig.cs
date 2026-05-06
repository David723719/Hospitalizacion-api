namespace HospitalizacionAPI.Models;

public class ExternalServicesConfig
{
    public ServiceConfig Farmacia { get; set; } = new();
    public ServiceConfig Emergencias { get; set; } = new();
    public ServiceConfig RecursosHumanos { get; set; } = new();
    public ServiceConfig Facturacion { get; set; } = new();
    public ServiceConfig Logistica { get; set; } = new();
}

public class ServiceConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}