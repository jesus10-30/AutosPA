namespace AutosPAService.Entities;

public partial class RechazoCita
{
    public long IdRechazo { get; set; }

    public long IdCita { get; set; }

    public long IdAgente { get; set; }

    public string Motivo { get; set; } = null!;

    public DateTime? FechaRechazo { get; set; }

    public virtual Cita CitaNavigation { get; set; } = null!;

    public virtual Usuario AgenteNavigation { get; set; } = null!;
}
