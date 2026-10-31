namespace AutosPALibrary.Dtos;

public class RegistrarCitaDto
{
    public long IdCliente { get; set; }

    public long IdVehiculo { get; set; }

    public long IdTipoServicio { get; set; }

    public long? IdAgenteSeleccionado { get; set; }

    public DateOnly FechaCita { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public string? Observaciones { get; set; }
}
