using AutosPAService.Services;
using Microsoft.AspNetCore.Mvc;
using AutosPAService.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AutosPAService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiculosController : ControllerBase
{
    private readonly VehiculosService _vehiculosService;

    public VehiculosController(VehiculosService vehiculosService)
    {
        _vehiculosService = vehiculosService;
    }

    private IActionResult? ValidarUsuarioSolicitante(long idUsuarioSolicitado)
    {
        var idTexto = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(idTexto, out var idUsuarioAutenticado))
            return Unauthorized();

        return idUsuarioAutenticado == idUsuarioSolicitado
            ? null
            : Forbid();
    }

    [HttpGet("marcas")]
    public async Task<IActionResult> ObtenerMarcas()
    {
        var marcas = await _vehiculosService.ObtenerMarcasActivasAsync();

        return Ok(marcas.Select(m => new
        {
            m.IdMarca,
            m.Nombre
        }));
    }

    [HttpGet("tipos")]
    public async Task<IActionResult> ObtenerTipos()
    {
        var tipos = await _vehiculosService.ObtenerTiposActivosAsync();

        return Ok(tipos.Select(t => new
        {
            t.IdTipoVehiculo,
            t.Nombre,
            t.Descripcion
        }));
    }

    [HttpGet("usuario/{idUsuario:long}")]
    [Authorize]
    public async Task<IActionResult> ObtenerPorUsuario(long idUsuario)
    {
        var errorAutorizacion = ValidarUsuarioSolicitante(idUsuario);
        if (errorAutorizacion is not null)
            return errorAutorizacion;

        var vehiculos = await _vehiculosService.ObtenerPorUsuarioAsync(idUsuario);

        return Ok(vehiculos);
    }

[HttpPost("usuario/{idUsuario:long}")]
[Authorize]
public async Task<IActionResult> CrearVehiculo(
    long idUsuario,
    [FromBody] CrearVehiculoDTO dto)
{
    var errorAutorizacion = ValidarUsuarioSolicitante(idUsuario);
    if (errorAutorizacion is not null)
        return errorAutorizacion;

    if (string.IsNullOrWhiteSpace(dto.Modelo) ||
        string.IsNullOrWhiteSpace(dto.Placa) ||
        dto.Anio <= 0 ||
        dto.Kilometraje < 0)
    {
        return BadRequest(new
        {
            mensaje = "Verifica los datos del vehÃ­culo."
        });
    }

    var resultado = await _vehiculosService.CrearAsync(idUsuario, dto);

    if (!resultado.Exito)
        return BadRequest(new { mensaje = resultado.Mensaje });

    return Ok(new { mensaje = resultado.Mensaje });
}


    [HttpDelete("usuario/{idUsuario:long}/{idVehiculo:long}")]
    [Authorize]
    public async Task<IActionResult> EliminarVehiculo(
        long idUsuario,
        long idVehiculo)
    {
        var errorAutorizacion = ValidarUsuarioSolicitante(idUsuario);
        if (errorAutorizacion is not null)
            return errorAutorizacion;

        var eliminado = await _vehiculosService.EliminarPorUsuarioAsync(
            idVehiculo,
            idUsuario);

        if (!eliminado)
            return NotFound(new { mensaje = "No se encontró el vehículo." });

        return Ok(new { mensaje = "Vehículo eliminado correctamente." });
    }}
