namespace HospitalizacionAPI.Models;

// 🟢 FARMACIA
public class MedicamentoDto { public string Codigo { get; set; } = ""; public string Nombre { get; set; } = ""; public int Stock { get; set; } }
public class StockResponseDto { public string Codigo { get; set; } = ""; public int StockDisponible { get; set; } public bool Suficiente { get; set; } }

// 🟡 EMERGENCIAS
public class PacienteTriageDto { public string Codigo { get; set; } = ""; public string Nombre { get; set; } = ""; public string NivelUrgencia { get; set; } = ""; }

// 🔵 RECURSOS HUMANOS
public class MedicoDto { public string Codigo { get; set; } = ""; public string Nombre { get; set; } = ""; public string Especialidad { get; set; } = ""; public bool Disponible { get; set; } }

// 🟣 FACTURACIÓN
public class SeguroDto { public string PacienteCodigo { get; set; } = ""; public string Aseguradora { get; set; } = ""; public bool CoberturaActiva { get; set; } public decimal Copago { get; set; } }

// 📦 LOGÍSTICA (Camas que te asignan)
public class CamaLogisticaDto {
    public string CodigoLogistica { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}

// 📦 RESPUESTA GENÉRICA
public class RespuestaApiDto { public bool Success { get; set; } public string Mensaje { get; set; } = ""; public object? Data { get; set; } }