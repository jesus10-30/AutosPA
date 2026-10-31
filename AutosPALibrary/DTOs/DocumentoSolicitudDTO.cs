namespace AutosPALibrary.DTOs;

public class DocumentoSolicitudDTO
{
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public bool EstadoActivo { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
}
