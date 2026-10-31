using AutosPAService.Interfaces;
using AutosPALibrary.DTOs;

namespace AutosPAService.Services;

public enum ResultadoSolicitudService
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

public class AgenteService
{
    private readonly IAgenteRepository _repository;

    public AgenteService(IAgenteRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> EsAgenteAsync(long idUsuario)
    {
        return _repository.EsAgenteAsync(idUsuario);
    }

    public Task<List<AgenteTrabajoDTO>> ObtenerTrabajosAsync(long idAgente)
    {
        return _repository.ObtenerTrabajosAsync(idAgente);
    }

    public Task<List<AgenteTrabajoDTO>> ObtenerTrabajosAsync(
        long idAgente,
        DateOnly desde,
        DateOnly hasta)
    {
        return _repository.ObtenerTrabajosAsync(idAgente, desde, hasta);
    }

    public Task<List<SolicitudAgenteDTO>> ObtenerSolicitudesAsync(long idAgente)
    {
        return _repository.ObtenerSolicitudesAsync(idAgente);
    }

    public async Task<ResultadoSolicitudService> AceptarSolicitudAsync(
        long idCita,
        long idAgente)
    {
        var resultado = await _repository.AceptarSolicitudAsync(idCita, idAgente);
        return TraducirResultado(resultado);
    }

    public async Task<ResultadoSolicitudService> RechazarSolicitudAsync(
        long idCita,
        long idAgente,
        RechazarSolicitudDTO solicitud)
    {
        if (solicitud is null || string.IsNullOrWhiteSpace(solicitud.Motivo))
            return ResultadoSolicitudService.MotivoInvalido;

        var resultado = await _repository.RechazarSolicitudAsync(
            idCita,
            idAgente,
            solicitud);

        return TraducirResultado(resultado);
    }

    public async Task<ResultadoSolicitudService> DeshacerSolicitudAsync(
        long idCita,
        long idAgente)
    {
        var resultado = await _repository.DeshacerSolicitudAsync(idCita, idAgente);
        return TraducirResultado(resultado);
    }

    private static ResultadoSolicitudService TraducirResultado(
        ResultadoOperacionSolicitud resultado)
    {
        return resultado switch
        {
            ResultadoOperacionSolicitud.Exito => ResultadoSolicitudService.Exito,
            ResultadoOperacionSolicitud.SolicitudInexistente => ResultadoSolicitudService.SolicitudInexistente,
            ResultadoOperacionSolicitud.SolicitudNoAsignada => ResultadoSolicitudService.SolicitudNoAsignada,
            ResultadoOperacionSolicitud.EstadoInvalido => ResultadoSolicitudService.EstadoInvalido,
            ResultadoOperacionSolicitud.MotivoInvalido => ResultadoSolicitudService.MotivoInvalido,
            ResultadoOperacionSolicitud.DuracionInvalida => ResultadoSolicitudService.DuracionInvalida,
            ResultadoOperacionSolicitud.ConflictoHorario => ResultadoSolicitudService.ConflictoHorario,
            ResultadoOperacionSolicitud.ConflictoConcurrencia => ResultadoSolicitudService.ConflictoConcurrencia,
            _ => throw new ArgumentOutOfRangeException(nameof(resultado), resultado, "Resultado de solicitud no reconocido.")
        };
    }

    public Task<bool> IniciarTrabajoAsync(long idCita, long idAgente)
    {
        return _repository.IniciarTrabajoAsync(idCita, idAgente);
    }

    public Task<bool> FinalizarTrabajoAsync(long idCita, long idAgente)
    {
        return _repository.FinalizarTrabajoAsync(idCita, idAgente);
    }

    public Task<PerfilAgenteDTO?> ObtenerPerfilAsync(long idUsuario)
    {
        return _repository.ObtenerPerfilAsync(idUsuario);
    }

    public async Task<ResultadoActualizacionPerfil> ActualizarPerfilAsync(
        long idUsuario,
        ActualizarPerfilAgenteDTO perfil)
    {
        var perfilActual = await _repository.ObtenerPerfilAsync(idUsuario);
        if (perfilActual is null)
            return ResultadoActualizacionPerfil.UsuarioNoEncontrado;

        if (!string.Equals(perfilActual.Email.Trim(), perfil.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            if (await _repository.CorreoEnUsoAsync(idUsuario, perfil.Email))
                return ResultadoActualizacionPerfil.CorreoDuplicado;

            return ResultadoActualizacionPerfil.CambioCorreoRequiereSincronizacionAuth;
        }

        var actualizado = await _repository.ActualizarPerfilAsync(idUsuario, perfil);
        return actualizado
            ? ResultadoActualizacionPerfil.Exito
            : ResultadoActualizacionPerfil.UsuarioNoEncontrado;
    }
}
