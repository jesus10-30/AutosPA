using AutosPALibrary.DTOs;

namespace AutosPAService.Interfaces;

public enum ResultadoOperacionSolicitud
{
    Exito,
    SolicitudInexistente,
    SolicitudNoAsignada,
    EstadoInvalido,
    MotivoInvalido,
    DuracionInvalida,
    ConflictoHorario,
    ConflictoConcurrencia
}

public interface IAgenteRepository
{
    Task<List<SolicitudAgenteDTO>> ObtenerSolicitudesAsync(long idAgente);
    Task<ResultadoOperacionSolicitud> AceptarSolicitudAsync(long idCita, long idAgente);
    Task<ResultadoOperacionSolicitud> RechazarSolicitudAsync(
        long idCita,
        long idAgente,
        RechazarSolicitudDTO solicitud);
    Task<ResultadoOperacionSolicitud> DeshacerSolicitudAsync(long idCita, long idAgente);
    Task<List<AgenteTrabajoDTO>> ObtenerTrabajosAsync(long idAgente);
    Task<List<AgenteTrabajoDTO>> ObtenerTrabajosAsync(
        long idAgente,
        DateOnly desde,
        DateOnly hasta);
    Task<bool> EsAgenteAsync(long idUsuario);
    Task<bool> IniciarTrabajoAsync(long idCita, long idAgente);
    Task<bool> FinalizarTrabajoAsync(long idCita, long idAgente);

    Task<PerfilAgenteDTO?> ObtenerPerfilAsync(long idUsuario);
    Task<bool> ActualizarPerfilAsync(long idUsuario, ActualizarPerfilAgenteDTO perfil);
    Task<bool> CorreoEnUsoAsync(long idUsuario, string email);
}
