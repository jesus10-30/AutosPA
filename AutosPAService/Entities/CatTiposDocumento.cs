using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class CatTiposDocumento
{
    public long IdTipoDocumento { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<DocumentosVehiculo> DocumentosVehiculos { get; set; } = new List<DocumentosVehiculo>();
}
