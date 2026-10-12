using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class Cita
{
    public long IdCita { get; set; }

    public long IdCliente { get; set; }

    public long? IdAgente { get; set; }

    public long IdVehiculo { get; set; }

    public long IdTipoServicio { get; set; }

    public long IdEstadoCita { get; set; }

    public DateOnly FechaCita { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public decimal Costo { get; set; }

    public int TiempoEstimadoMinutos { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Usuario? IdAgenteNavigation { get; set; }

    public virtual Usuario IdClienteNavigation { get; set; } = null!;

    public virtual CatEstadosCitum IdEstadoCitaNavigation { get; set; } = null!;

    public virtual CatTiposServicio IdTipoServicioNavigation { get; set; } = null!;

    public virtual Vehiculo IdVehiculoNavigation { get; set; } = null!;

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();
}
