namespace AutosPALibrary.DTOs;

public class NotificacionDTO
{
    public long Id { get; set; }
    public long? IdCita { get; set; }
    public long? IdTipoNotificacion { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool Leida { get; set; }
}