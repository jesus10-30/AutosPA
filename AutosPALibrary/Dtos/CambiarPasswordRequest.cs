
namespace AutosPALibrary.Dtos;

public class CambiarPasswordRequest
{
    public long IdUsuario { get; set; }

    public string PasswordActual { get; set; } = string.Empty;

    public string PasswordNueva { get; set; } = string.Empty;
}
