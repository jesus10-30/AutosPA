using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class Vehiculo
{
    public long IdVehiculo { get; set; }

    public long IdConductor { get; set; }

    public long IdTipoVehiculo { get; set; }

    public long IdMarca { get; set; }

    public string Modelo { get; set; } = null!;

    public int Anio { get; set; }

    public string Placa { get; set; } = null!;

    public string? Color { get; set; }

    public int Kilometraje { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public virtual ICollection<DocumentosVehiculo> DocumentosVehiculos { get; set; } = new List<DocumentosVehiculo>();

    public virtual Conductore IdConductorNavigation { get; set; } = null!;

    public virtual CatMarca IdMarcaNavigation { get; set; } = null!;

    public virtual CatTiposVehiculo IdTipoVehiculoNavigation { get; set; } = null!;
}
