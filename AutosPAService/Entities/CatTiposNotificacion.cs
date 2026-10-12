using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class CatTiposNotificacion
{
    public long IdTipoNotificacion { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }
}
