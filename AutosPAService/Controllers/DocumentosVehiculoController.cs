using System.Security.Claims;
using AutosPAService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutosPAService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentosVehiculoController : ControllerBase
{
    private readonly DocumentosVehiculoService _service;

    public DocumentosVehiculoController(DocumentosVehiculoService service)
    {
        _service = service;
    }

    private bool ObtenerUsuario(out long idUsuario)
    {
        return long.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out idUsuario);
    }

    private bool ObtenerIdentidadSupabase(out Guid authUserId)
    {
        return Guid.TryParse(
            User.FindFirstValue("supabase_user_id"),
            out authUserId);
    }

    private string ObtenerToken()
    {
        var authorization = Request.Headers.Authorization.ToString();
        return authorization.StartsWith("Bearer ",
            StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..].Trim()
            : "";
    }

    [HttpGet("tipos")]
    public async Task<IActionResult> ObtenerTipos()
    {
        var tipos = await _service.ObtenerTiposAsync();

        return Ok(tipos.Select(t => new
        {
            t.IdTipoDocumento,
            t.Nombre,
            t.Descripcion
        }));
    }

    [HttpGet("vehiculo/{idVehiculo:long}")]
    public async Task<IActionResult> ObtenerPorVehiculo(long idVehiculo)
    {
        if (!ObtenerUsuario(out var idUsuario))
            return Unauthorized();

        var documentos = await _service.ObtenerPorVehiculoAsync(
            idVehiculo, idUsuario);

        if (documentos is null)
            return NotFound(new { mensaje = "Vehículo no encontrado." });

        return Ok(documentos);
    }

    [HttpPost("vehiculo/{idVehiculo:long}")]
[Consumes("multipart/form-data")]
[RequestSizeLimit(6 * 1024 * 1024)]
public async Task<IActionResult> Subir(
    long idVehiculo,
    [FromForm] long idTipoDocumento,
    [FromForm] DateTime? fechaVencimiento,
    IFormFile archivo)
{
    if (!ObtenerUsuario(out var idUsuario) ||
        !ObtenerIdentidadSupabase(out var authUserId))
        return Unauthorized();

    var resultado = await _service.SubirAsync(
        idVehiculo,
        idUsuario,
        authUserId,
        idTipoDocumento,
        fechaVencimiento,
        archivo,
        ObtenerToken());

    if (!resultado.Exito)
        return BadRequest(new { mensaje = resultado.Mensaje });

    return Ok(new { mensaje = resultado.Mensaje });
}

    [HttpDelete("{idDocumento:long}")]
    public async Task<IActionResult> Eliminar(long idDocumento)
    {
        if (!ObtenerUsuario(out var idUsuario))
            return Unauthorized();

        var resultado = await _service.EliminarAsync(
            idDocumento, idUsuario, ObtenerToken());

        if (!resultado.Exito)
            return BadRequest(new { mensaje = resultado.Mensaje });

        return Ok(new { mensaje = resultado.Mensaje });
    }
}