using AutosPA.models;

namespace AutosPA.Services
{
    public interface IAgenteService
    {
        Task<Agente> ObtenerAgenteActualAsync();
        Task<List<Trabajo>> ObtenerTrabajosAsync(DateTime desde, DateTime hasta);
        Task<List<Solicitud>> ObtenerSolicitudesAsync();                // todas (página Solicitudes)
        Task<List<Solicitud>> ObtenerSolicitudesPendientesAsync();      // solo pendientes (panel)
        Task<List<Notificacion>> ObtenerNotificacionesAsync();

        Task CambiarDisponibilidadAsync(bool disponible);
        Task IniciarTrabajoAsync(int trabajoId, DateTime inicio);
        Task FinalizarTrabajoAsync(int trabajoId, decimal costoFinal, int duracionMin, string notas);
        Task GuardarNotasAsync(int trabajoId, string notas);

        Task AceptarSolicitudAsync(int solicitudId);                    // crea el trabajo en la agenda
        Task RechazarSolicitudAsync(int solicitudId, string motivo);
        Task DeshacerSolicitudAsync(int solicitudId);                   // vuelve a Pendiente y quita el trabajo

        Task MarcarNotificacionLeidaAsync(int notificacionId);
        Task MarcarTodasLeidasAsync();
    }
}
