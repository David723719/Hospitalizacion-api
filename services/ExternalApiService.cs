using System.Text.Json;
using HospitalizacionAPI.Models;
using Microsoft.Extensions.Options;

namespace HospitalizacionAPI.Services;

public class ExternalApiService
{
    private readonly IHttpClientFactory _http;
    private readonly IOptionsMonitor<ExternalServicesConfig> _cfg;

    public ExternalApiService(IHttpClientFactory http, IOptionsMonitor<ExternalServicesConfig> cfg)
    {
        _http = http; 
        _cfg = cfg;
    }

    private ServiceConfig GetConfig(string svc) => svc.ToLower() switch
    {
        "farmacia" => _cfg.CurrentValue.Farmacia,
        "emergencias" => _cfg.CurrentValue.Emergencias,
        "recursoshumanos" => _cfg.CurrentValue.RecursosHumanos,
        "facturacion" => _cfg.CurrentValue.Facturacion,
        "logistica" => _cfg.CurrentValue.Logistica,
        _ => throw new ArgumentException($"Servicio '{svc}' no configurado")
    };

    private HttpClient GetClient(string svc)
    {
        var c = GetConfig(svc);
        if (!c.Enabled || string.IsNullOrWhiteSpace(c.BaseUrl))
            throw new InvalidOperationException($"Servicio '{svc}' no habilitado");

        var client = _http.CreateClient();
        client.BaseAddress = new Uri(c.BaseUrl);
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    private async Task<T?> GetAsync<T>(string svc, string ep)
    {
        using var client = GetClient(svc);
        var res = await client.GetAsync(ep);
        res.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(), 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // 🟢 FARMACIA - ENDPOINTS REALES
    public async Task<List<MedicamentoDto>?> GetMedicamentosCatalogo() =>
        await GetAsync<List<MedicamentoDto>>("farmacia", "/api/Medicamentos/catalogo");

    public async Task<StockDto?> GetDisponibilidadMedicamento(string codigo) =>
        await GetAsync<StockDto>("farmacia", $"/api/StocksActuales/disponibilidad/{codigo}");

    // Otros métodos existentes...
    public async Task<List<PacienteTriageDto>?> GetTriaje() => 
        await GetAsync<List<PacienteTriageDto>>("emergencias", "/api/triaje/pendientes");
    
    public async Task<List<MedicoDto>?> GetMedicos(string esp = "", bool disp = true) => 
        await GetAsync<List<MedicoDto>>("recursoshumanos", $"/api/doctores?especialidad={Uri.EscapeDataString(esp)}&disponibles={disp}");
    
    public async Task<SeguroDto?> ValidarSeguro(string pacienteCod) => 
        await GetAsync<SeguroDto>("facturacion", $"/api/seguros/validar/{pacienteCod}");
}

// DTOs para Farmacia
public class StockDto {
    public string MedicamentoCodigo { get; set; } = "";
    public int StockDisponible { get; set; }
    public bool Disponible { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}