using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class Notificacione
{
    public long Id { get; set; }

    public long IdCliente { get; set; }

    public long? IdCita { get; set; }

    public string Mensaje { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? Via { get; set; }

    public DateOnly? FechaEnvio { get; set; }

    public string? Estado { get; set; }

    public virtual Cita? IdCitaNavigation { get; set; }

    public virtual Usuario IdClienteNavigation { get; set; } = null!;
}
