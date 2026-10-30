namespace AutosPAService.DTOs;

public class VehiculoDTO
{
    public long IdVehiculo { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int Kilometraje { get; set; }
}