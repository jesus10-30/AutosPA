namespace AutosPALibrary.DTOs;

public class ServicioDTO
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal CostoBase { get; set; }
    public int TiempoEstimadoMinutos { get; set; }
}