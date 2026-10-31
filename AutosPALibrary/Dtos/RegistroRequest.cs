
namespace AutosPALibrary.Dtos;

public class RegistroRequest
{
    // Datos personales
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    // Cuenta
    public string Password { get; set; } = string.Empty;

    // Vehículo
    public bool OmitirVehiculo { get; set; }
    public long? IdMarca { get; set; }
    public string? Modelo { get; set; }
    public int? Anio { get; set; }
    public string? Placa { get; set; }
    public long? IdTipoVehiculo { get; set; }
    public string? Color { get; set; }


    // Conductor: en esta opción es el mismo usuario
    public string NumeroLicencia { get; set; } = string.Empty;
    public DateOnly? FechaVencimientoLicencia { get; set; }
}
