using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class CatEstadosDocumento
{
    public long IdEstadoDocumento { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<DocumentosVehiculo> DocumentosVehiculos { get; set; } = new List<DocumentosVehiculo>();
}
