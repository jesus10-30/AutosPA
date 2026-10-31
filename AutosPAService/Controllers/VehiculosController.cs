using AutosPAService.Services;
using Microsoft.AspNetCore.Mvc;
using AutosPAService.DTOs;

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
    public async Task<IActionResult> ObtenerPorUsuario(long idUsuario)
    {
        var vehiculos = await _vehiculosService.ObtenerPorUsuarioAsync(idUsuario);

        return Ok(vehiculos);
    }

[HttpPost("usuario/{idUsuario:long}")]
public async Task<IActionResult> CrearVehiculo(
    long idUsuario,
    [FromBody] CrearVehiculoDTO dto)
{
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
    public async Task<IActionResult> EliminarVehiculo(
        long idUsuario,
        long idVehiculo)
    {
        var eliminado = await _vehiculosService.EliminarPorUsuarioAsync(
            idVehiculo,
            idUsuario);

        if (!eliminado)
            return NotFound(new { mensaje = "No se encontró el vehículo." });

        return Ok(new { mensaje = "Vehículo eliminado correctamente." });
    }}
