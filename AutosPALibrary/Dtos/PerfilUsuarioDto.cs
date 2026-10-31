
namespace AutosPALibrary.Dtos;

public class PerfilUsuarioDto
{
    public long IdUsuario { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public DateTime FechaRegistro { get; set; }


    public string Rol { get; set; } = string.Empty;
}
