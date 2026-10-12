using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class CatTiposServicio
{
    public long IdTipoServicio { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal CostoBase { get; set; }

    public int TiempoEstimadoMinutos { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();
}
