using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class CatTiposVehiculo
{
    public long IdTipoVehiculo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
