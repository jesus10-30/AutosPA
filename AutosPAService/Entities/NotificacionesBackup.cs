using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class NotificacionesBackup
{
    public long? IdNotificacion { get; set; }

    public long? IdUsuario { get; set; }

    public long? IdTipoNotificacion { get; set; }

    public long? IdCita { get; set; }

    public string? Titulo { get; set; }

    public string? Mensaje { get; set; }

    public bool? Leida { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaLectura { get; set; }
}
