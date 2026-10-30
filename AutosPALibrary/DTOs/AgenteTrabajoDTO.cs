namespace AutosPALibrary.DTOs;

public class AgenteTrabajoDTO
{
    public long IdCita { get; set; }
    public DateOnly FechaCita { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Vehiculo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Servicio { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal Costo { get; set; }
    public int TiempoEstimadoMinutos { get; set; }
    public string? Observaciones { get; set; }
}