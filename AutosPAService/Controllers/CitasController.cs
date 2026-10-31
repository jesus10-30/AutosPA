using AutosPAService.DTOs;
using AutosPAService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutosPAService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitasController : ControllerBase
{
    private readonly CitasService _citasService;

    public CitasController(CitasService citasService)
    {
        _citasService = citasService;
    }

    [HttpGet("cliente/{idCliente:long}/historial")]
    public async Task<IActionResult> ObtenerPorCliente(long idCliente)
    {
        var citas = await _citasService.ObtenerPorClienteAsync(idCliente);
        return Ok(citas);
    }

    [HttpPost("cliente/{idCliente:long}")]
    public async Task<IActionResult> Crear(
        long idCliente,
        [FromBody] CrearCitaDTO datos)
    {
        if (datos.FechaCita < DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { mensaje = "La fecha de la cita no puede estar en el pasado." });

        var cita = await _citasService.CrearAsync(idCliente, datos);

        if (cita is null)
            return BadRequest(new
            {
                mensaje = "El vehículo no pertenece al cliente o el servicio no está disponible."
            });

        return CreatedAtAction(
            nameof(ObtenerPorCliente),
            new { idCliente },
            cita);
    }

[HttpDelete("cliente/{idCliente:long}/{idCita:long}")]
public async Task<IActionResult> Cancelar(long idCliente, long idCita)
{
    var cancelada = await _citasService.CancelarAsync(idCliente, idCita);

    if (!cancelada)
        return NotFound(new
        {
            mensaje = "No se encontró la cita, no pertenece al cliente o no se puede cancelar."
        });

    return NoContent();
}

}
