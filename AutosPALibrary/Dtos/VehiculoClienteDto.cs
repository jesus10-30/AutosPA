
namespace AutosPALibrary.Dtos;

public class VehiculoClienteDto
{
    public long IdVehiculo { get; set; }

    public string Placa { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int Anio { get; set; }

    public string? Color { get; set; }

    public int Kilometraje { get; set; }

    public string Etiqueta => $"{Marca} {Modelo} — {Placa}";
}
