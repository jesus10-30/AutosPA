namespace AutosPALibrary.DTOs;

public class PerfilClienteDTO
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public int TotalVehiculos { get; set; }
    public int CitasProximas { get; set; }
    public int ServiciosCompletados { get; set; }
}