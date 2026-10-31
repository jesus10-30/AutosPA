namespace AutosPAService.DTOs;

public class DocumentoVehiculoDTO
{
    public long IdDocumento { get; set; }
    public long IdTipoDocumento { get; set; }
    public string Tipo { get; set; } = "";
    public string Archivo { get; set; } = "";
    public DateTime? FechaVencimiento { get; set; }
    public string Estado { get; set; } = "";
}