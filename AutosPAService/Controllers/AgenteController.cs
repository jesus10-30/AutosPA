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
    public async Task<IActionResult> ObtenerTrabajos()
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

        var trabajos = await _service.ObtenerTrabajosAsync(idAgente.Value);

        return Ok(trabajos);
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

        var actualizado = await _service.ActualizarPerfilAsync(
            idAgente.Value, perfil);

        if (!actualizado)
            return NotFound();

        return Ok(new { mensaje = "Perfil actualizado correctamente." });
    }
}