using System.ComponentModel.DataAnnotations;

namespace AutosPAService.DTOs;

public class CrearCitaDTO
{
    [Required]
    public long IdVehiculo { get; set; }

    [Required]
    public long IdTipoServicio { get; set; }

    [Required]
    public DateOnly FechaCita { get; set; }

    [Required]
    public TimeOnly HoraInicio { get; set; }

    public string? Observaciones { get; set; }
}
