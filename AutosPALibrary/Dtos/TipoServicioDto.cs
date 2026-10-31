
namespace AutosPALibrary.Dtos;

public class TipoServicioDto
{
    public long IdTipoServicio { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal CostoBase { get; set; }

    public int TiempoEstimadoMinutos { get; set; }
}
