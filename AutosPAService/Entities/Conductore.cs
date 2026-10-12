using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class Conductore
{
    public long IdConductor { get; set; }

    public long IdUsuario { get; set; }

    public string NumeroLicencia { get; set; } = null!;

    public DateOnly? FechaVencimientoLicencia { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
