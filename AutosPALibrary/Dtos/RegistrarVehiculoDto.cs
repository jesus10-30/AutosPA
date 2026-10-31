
namespace AutosPALibrary.Dtos;

public class RegistrarVehiculoDto
{
    public long IdCliente { get; set; }
    public long IdMarca { get; set; }
    public long IdTipoVehiculo { get; set; }

    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int Kilometraje { get; set; }
}
