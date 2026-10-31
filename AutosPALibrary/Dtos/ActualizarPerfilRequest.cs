
namespace AutosPALibrary.Dtos;

public class ActualizarPerfilRequest
{
    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string? Telefono { get; set; }
}
