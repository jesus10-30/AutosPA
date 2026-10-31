using System.Security.Claims;
using AutosPAService.Services;
using AutosPALibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutosPAService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgenteController : ControllerBase
{
    private readonly AgenteService _service;

    public AgenteController(AgenteService service)
    {
        _service = service;
    }

    private async Task<long?> ObtenerIdAgenteAsync()
    {
        var idUsuarioTexto = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!long.TryParse(idUsuarioTexto, out var idAgente))
            return null;

        var esAgente = await _service.EsAgenteAsync(idAgente);

        return esAgente ? idAgente : null;
    }

    [HttpGet("trabajos")]
    public async Task<IActionResult> ObtenerTrabajos(
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta)
    {
        var idAgente = await ObtenerIdAgenteAsync();

        if (idAgente == null)
        {
            if (!long.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out _))
                return Unauthorized();

            return Forbid();
        }

        if (desde.HasValue != hasta.HasValue)
            return BadRequest("Debe proporcionar ambas fechas: desde y hasta.");

        if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            return BadRequest("La fecha desde no puede ser posterior a la fecha hasta.");

        var trabajos = desde.HasValue && hasta.HasValue
            ? await _service.ObtenerTrabajosAsync(idAgente.Value, desde.Value, hasta.Value)
            : await _service.ObtenerTrabajosAsync(idAgente.Value);

        return Ok(trabajos);
    }

    [HttpGet("solicitudes")]
    public async Task<IActionResult> ObtenerSolicitudes()
    {
        var idAgente = await ObtenerIdAgenteAsync();

        if (idAgente == null)
            return Forbid();

        var solicitudes = await _service.ObtenerSolicitudesAsync(idAgente.Value);
        return Ok(solicitudes);
    }

    [HttpPut("solicitudes/{idCita:long}/aceptar")]
    public async Task<IActionResult> AceptarSolicitud(long idCita)
    {
        var idAgente = await ObtenerIdAgenteAsync();

        if (idAgente == null)
            return Forbid();

        var resultado = await _service.AceptarSolicitudAsync(idCita, idAgente.Value);
        return ResponderOperacionSolicitud(resultado, "Solicitud aceptada correctamente.");
    }

    [HttpPut("solicitudes/{idCita:long}/rechazar")]
    public async Task<IActionResult> RechazarSolicitud(
        long idCita,
        [FromBody] RechazarSolicitudDTO solicitud)
    {
        var idAgente = await ObtenerIdAgenteAsync();

        if (idAgente == null)
            return Forbid();

        var resultado = await _service.RechazarSolicitudAsync(
            idCita,
            idAgente.Value,
            solicitud);

        return ResponderOperacionSolicitud(resultado, "Solicitud rechazada correctamente.");
    }

    [HttpPut("solicitudes/{idCita:long}/deshacer")]
    public async Task<IActionResult> DeshacerSolicitud(long idCita)
    {
        var idAgente = await ObtenerIdAgenteAsync();

        if (idAgente == null)
            return Forbid();

        var resultado = await _service.DeshacerSolicitudAsync(idCita, idAgente.Value);
        return ResponderOperacionSolicitud(resultado, "Se deshizo la acción correctamente.");
    }

    private IActionResult ResponderOperacionSolicitud(
        ResultadoSolicitudService resultado,
        string mensajeExito)
    {
        return resultado switch
        {
            ResultadoSolicitudService.Exito => Ok(new { mensaje = mensajeExito }),
            ResultadoSolicitudService.SolicitudInexistente => NotFound(),
            ResultadoSolicitudService.SolicitudNoAsignada => Forbid(),
            ResultadoSolicitudService.EstadoInvalido => Conflict("El estado actual de la solicitud no permite esta operación."),
            ResultadoSolicitudService.MotivoInvalido => BadRequest("El motivo de rechazo es obligatorio."),
            ResultadoSolicitudService.DuracionInvalida => Conflict("La solicitud o un trabajo existente tiene una duración inválida. No se puede validar la agenda."),
            ResultadoSolicitudService.ConflictoHorario => Conflict("La solicitud tiene un conflicto de horario."),
            ResultadoSolicitudService.ConflictoConcurrencia => Conflict("La solicitud cambió mientras se procesaba. Vuelve a cargarla e inténtalo de nuevo."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPut("trabajos/{idCita}/iniciar")]
    public async Task<IActionResult> IniciarTrabajo(long idCita)
    {
        var idAgente = await ObtenerIdAgenteAsync();

        if (idAgente == null)
            return Forbid();

        var resultado = await _service.IniciarTrabajoAsync(
            idCita, idAgente.Value);

        if (!resultado)
            return BadRequest("No se pudo iniciar el trabajo.");

        return Ok(new { mensaje = "Trabajo iniciado correctamente." });
    }

    [HttpPut("trabajos/{idCita}/finalizar")]
    public async Task<IActionResult> FinalizarTrabajo(long idCita)
    {
        var idAgente = await ObtenerIdAgenteAsync();

        if (idAgente == null)
            return Forbid();

        var resultado = await _service.FinalizarTrabajoAsync(
            idCita, idAgente.Value);

        if (!resultado)
            return BadRequest("No se pudo finalizar el trabajo.");

        return Ok(new { mensaje = "Trabajo finalizado correctamente." });
    }

    [HttpGet("perfil")]
    public async Task<IActionResult> ObtenerPerfil()
    {
        var idAgente = await ObtenerIdAgenteAsync();

        if (idAgente == null)
            return Forbid();

        var perfil = await _service.ObtenerPerfilAsync(idAgente.Value);

        if (perfil == null)
            return NotFound();

        return Ok(perfil);
    }

    [HttpPut("perfil")]
    public async Task<IActionResult> ActualizarPerfil(
        [FromBody] ActualizarPerfilAgenteDTO perfil)
    {
        var idAgente = await ObtenerIdAgenteAsync();

        if (idAgente == null)
            return Forbid();

        if (string.IsNullOrWhiteSpace(perfil.Nombres) ||
            string.IsNullOrWhiteSpace(perfil.Apellidos) ||
            string.IsNullOrWhiteSpace(perfil.Email))
        {
            return BadRequest("Nombres, apellidos y correo son obligatorios.");
        }

        var resultado = await _service.ActualizarPerfilAsync(
            idAgente.Value, perfil);

        return resultado switch
        {
            ResultadoActualizacionPerfil.Exito =>
                Ok(new { mensaje = "Perfil actualizado correctamente." }),
            ResultadoActualizacionPerfil.UsuarioNoEncontrado => NotFound(),
            ResultadoActualizacionPerfil.CorreoDuplicado =>
                Conflict("Ese correo ya está registrado."),
            ResultadoActualizacionPerfil.CambioCorreoRequiereSincronizacionAuth =>
                Conflict("El cambio de correo requiere sincronizar Supabase Auth y no está disponible todavía."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
