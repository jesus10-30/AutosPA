using System.ComponentModel.DataAnnotations;

namespace AutosPALibrary.DTOs;

public class CambiarPasswordDTO
{
    [Required]
    public string PasswordActual { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string PasswordNueva { get; set; } = string.Empty;
}
