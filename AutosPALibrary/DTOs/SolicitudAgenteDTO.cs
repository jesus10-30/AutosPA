namespace AutosPALibrary.DTOs;

public class SolicitudAgenteDTO
{
    public long IdCita { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Vehiculo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Servicio { get; set; } = string.Empty;
    public DateOnly FechaCita { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public decimal Costo { get; set; }
    public int TiempoEstimadoMinutos { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? MotivoRechazo { get; set; }
    public List<DocumentoSolicitudDTO> Documentos { get; set; } = new();
}
