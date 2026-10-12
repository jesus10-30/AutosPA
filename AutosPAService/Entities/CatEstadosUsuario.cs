using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class CatEstadosUsuario
{
    public long IdEstadoUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
