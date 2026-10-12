using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class CatMarca
{
    public long IdMarca { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
