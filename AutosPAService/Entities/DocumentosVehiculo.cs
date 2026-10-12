using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class DocumentosVehiculo
{
    public long IdDocumento { get; set; }

    public long IdVehiculo { get; set; }

    public long IdTipoDocumento { get; set; }

    public long IdEstadoDocumento { get; set; }

    public string? NumeroDocumento { get; set; }

    public string? NombreArchivo { get; set; }

    public string? UrlArchivo { get; set; }

    public DateOnly? FechaEmision { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual CatEstadosDocumento IdEstadoDocumentoNavigation { get; set; } = null!;

    public virtual CatTiposDocumento IdTipoDocumentoNavigation { get; set; } = null!;

    public virtual Vehiculo IdVehiculoNavigation { get; set; } = null!;
}
