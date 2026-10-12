using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class CatEstadosCitum
{
    public long IdEstadoCita { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();
}
