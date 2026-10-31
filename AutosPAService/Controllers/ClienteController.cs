using System.Security.Claims;
using AutosPAService.Services;
using AutosPALibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutosPAService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClienteController : ControllerBase
{
    private readonly ClienteService _service;

    public ClienteController(ClienteService service)
    {
        _service = service;
    }

    private long? ObtenerIdCliente()
    {
        var idTexto = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return long.TryParse(idTexto, out var id) ? id : null;
    }

    [HttpGet("perfil")]
    public async Task<IActionResult> ObtenerPerfil()
    {
        var idCliente = ObtenerIdCliente();

        if (idCliente == null)
            return Unauthorized();

        if (!await _service.EsClienteAsync(idCliente.Value))
            return Forbid();

        var perfil = await _service.ObtenerPerfilAsync(idCliente.Value);

        if (perfil == null)
            return NotFound("No se encontró el perfil del cliente.");

        return Ok(perfil);
    }

    [HttpPut("perfil")]
    public async Task<IActionResult> ActualizarPerfil(
        [FromBody] ActualizarPerfilClienteDTO perfil)
    {
        var idCliente = ObtenerIdCliente();

        if (idCliente == null)
            return Unauthorized();

        if (!await _service.EsClienteAsync(idCliente.Value))
            return Forbid();

        if (string.IsNullOrWhiteSpace(perfil.Nombres) ||
            string.IsNullOrWhiteSpace(perfil.Apellidos) ||
            string.IsNullOrWhiteSpace(perfil.Email))
        {
            return BadRequest(
                "Nombres, apellidos y correo son obligatorios.");
        }

        var resultado = await _service.ActualizarPerfilAsync(
            idCliente.Value, perfil);

        return resultado switch
        {
            ResultadoActualizacionPerfil.Exito =>
                Ok(new { mensaje = "Perfil actualizado correctamente." }),
            ResultadoActualizacionPerfil.UsuarioNoEncontrado =>
                NotFound("No se encontró el cliente."),
            ResultadoActualizacionPerfil.CorreoDuplicado =>
                Conflict("Ese correo ya está registrado."),
            ResultadoActualizacionPerfil.CambioCorreoRequiereSincronizacionAuth =>
                Conflict("El cambio de correo requiere sincronizar Supabase Auth y no está disponible todavía."),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
