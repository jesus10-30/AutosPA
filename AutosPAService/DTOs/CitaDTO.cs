namespace AutosPAService.DTOs;

public class CitaDTO
{
    public long IdCita { get; set; }
    public long IdVehiculo { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string ModeloVehiculo { get; set; } = string.Empty;
    public string Servicio { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateOnly FechaCita { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public decimal Costo { get; set; }
    public int TiempoEstimadoMinutos { get; set; }
    public string? Observaciones { get; set; }
}
