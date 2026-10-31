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

        if (string.IsNullOrWhiteSpace(perfil.Nombres) ||
            string.IsNullOrWhiteSpace(perfil.Apellidos) ||
            string.IsNullOrWhiteSpace(perfil.Email))
        {
            return BadRequest(
                "Nombres, apellidos y correo son obligatorios.");
        }

        var actualizado = await _service.ActualizarPerfilAsync(
            idCliente.Value, perfil);

        if (!actualizado)
            return NotFound("No se encontró el cliente.");

        return Ok(new { mensaje = "Perfil actualizado correctamente." });
    }
}