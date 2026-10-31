using System.ComponentModel.DataAnnotations;

namespace AutosPAService.DTOs;

/// <summary>
/// Campos multipart/form-data para subir un documento de vehículo.
/// </summary>
public sealed class SubirDocumentoVehiculoFormDTO
{
    [Required]
    public long IdTipoDocumento { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    [Required]
    public IFormFile Archivo { get; set; } = null!;
}
