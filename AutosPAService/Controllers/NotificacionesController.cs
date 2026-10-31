
using System.Security.Claims;
using AutosPAService.Interfaces;
using AutosPAService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutosPAService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionesService _notificacionesService;

    public NotificacionesController(
        INotificacionesService notificacionesService)
    {
        _notificacionesService = notificacionesService;
    }

    private bool IntentarObtenerIdCliente(out long idCliente)
    {
        var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(idUsuario, out idCliente);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerMisNotificaciones()
    {
        if (!IntentarObtenerIdCliente(out var idCliente))
            return Unauthorized();

        var notificaciones =
            await _notificacionesService.ObtenerPorClienteAsync(idCliente);

        return Ok(notificaciones);
    }

    [HttpPut("{idNotificacion:long}/leer")]
    public async Task<IActionResult> MarcarComoLeida(long idNotificacion)
    {
        if (!IntentarObtenerIdCliente(out var idCliente))
            return Unauthorized();

        var actualizada = await _notificacionesService
            .MarcarComoLeidaAsync(idNotificacion, idCliente);

        if (!actualizada)
            return NotFound(new { mensaje = "Notificación no encontrada." });

        return NoContent();
    }

    [HttpPut("leer-todas")]
    public async Task<IActionResult> MarcarTodasComoLeidas()
    {
        if (!IntentarObtenerIdCliente(out var idCliente))
            return Unauthorized();

        var cantidad = await _notificacionesService
            .MarcarTodasComoLeidasAsync(idCliente);

        return Ok(new { actualizadas = cantidad });
    }

    [HttpDelete("{idNotificacion:long}")]
    public async Task<IActionResult> EliminarNotificacion(long idNotificacion)
    {
        if (!IntentarObtenerIdCliente(out var idCliente))
            return Unauthorized();

        var eliminada = await _notificacionesService
            .EliminarPorClienteAsync(idNotificacion, idCliente);

        if (!eliminada)
            return NotFound(new
            {
                mensaje = "La notificación no existe o no te pertenece."
            });

        return NoContent();
    }
}
