using System;
using System.Collections.Generic;

namespace AutosPAService.Entities;

public partial class Usuario
{
    public long IdUsuario { get; set; }

    public long IdRol { get; set; }

    public long IdEstadoUsuario { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? PasswordHash { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public Guid? AuthUserId { get; set; }

    public virtual ICollection<Cita> CitaIdAgenteNavigations { get; set; } = new List<Cita>();

    public virtual ICollection<Cita> CitaIdClienteNavigations { get; set; } = new List<Cita>();

    public virtual Conductore? Conductore { get; set; }

    public virtual CatEstadosUsuario IdEstadoUsuarioNavigation { get; set; } = null!;

    public virtual CatRole IdRolNavigation { get; set; } = null!;

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();
}
