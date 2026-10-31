using AutosPALibrary.Dtos;
using AutosPAService.Services.Citas;
using Microsoft.AspNetCore.Mvc;

namespace AutosPAService.Controllers.Citas;

[ApiController]
[Route("api/[controller]")]
public class CitasController : ControllerBase
{
    private readonly ICitasService _citasService;

    public CitasController(ICitasService citasService)
    {
        _citasService = citasService;
    }

    [HttpGet("cliente/{idCliente:long}")]
    public async Task<ActionResult<List<CitaClienteDto>>> ObtenerCitasPorCliente(
        long idCliente)
    {
        if (idCliente <= 0)
        {
            return BadRequest(new
            {
                mensaje = "El ID del cliente no es válido."
            });
        }

        var citas = await _citasService.ObtenerCitasPorClienteAsync(idCliente);

        return Ok(citas);
    }

    [HttpGet("cliente/{idCliente:long}/vehiculos")]
    public async Task<ActionResult<List<VehiculoClienteDto>>>
        ObtenerVehiculosPorCliente(long idCliente)
    {
        if (idCliente <= 0)
            return BadRequest(new
            {
                mensaje = "El ID del cliente no es válido."
            });

        var vehiculos =
            await _citasService.ObtenerVehiculosPorClienteAsync(idCliente);

        return Ok(vehiculos);
    }

    [HttpGet("marcas")]
    public async Task<ActionResult<List<OpcionCatalogoDto>>> ObtenerMarcas()
    {
        var marcas = await _citasService.ObtenerMarcasActivasAsync();
        return Ok(marcas);
    }

    [HttpGet("tipos-vehiculo")]
    public async Task<ActionResult<List<OpcionCatalogoDto>>> ObtenerTiposVehiculo()
    {
        var tipos = await _citasService.ObtenerTiposVehiculoActivosAsync();
        return Ok(tipos);
    }

    [HttpPost("vehiculos")]
    public async Task<ActionResult<VehiculoClienteDto>> RegistrarVehiculo(
        [FromBody] RegistrarVehiculoDto dto)
    {
        if (dto.IdCliente <= 0)
            return BadRequest(new { mensaje = "El usuario no es válido." });

        if (dto.IdMarca <= 0 || dto.IdTipoVehiculo <= 0)
            return BadRequest(new { mensaje = "Selecciona una marca y un tipo de vehículo." });

        if (string.IsNullOrWhiteSpace(dto.Modelo) || dto.Modelo.Trim().Length > 100)
            return BadRequest(new { mensaje = "Ingresa un modelo válido de hasta 100 caracteres." });

        if (dto.Anio < 1886 || dto.Anio > DateTime.UtcNow.Year + 1)
            return BadRequest(new { mensaje = "El año del vehículo no es válido." });

        if (string.IsNullOrWhiteSpace(dto.Placa) || dto.Placa.Trim().Length > 20)
            return BadRequest(new { mensaje = "Ingresa una placa válida de hasta 20 caracteres." });

        if (dto.Color?.Length > 50)
            return BadRequest(new { mensaje = "El color no puede superar los 50 caracteres." });

        if (dto.Kilometraje < 0)
            return BadRequest(new { mensaje = "El kilometraje no puede ser negativo." });

        var (vehiculo, error) =
            await _citasService.RegistrarVehiculoAsync(dto);

        if (error is not null)
            return BadRequest(new { mensaje = error });

        return Ok(vehiculo);
    }
    [HttpPost]
    public async Task<ActionResult<CitaClienteDto>> RegistrarCita(
    [FromBody] RegistrarCitaDto dto)
    {
        if (dto.IdAgenteSeleccionado is null ||
            dto.IdAgenteSeleccionado <= 0)
        {
            return BadRequest(new
            {
                mensaje = "Debes seleccionar un agente."
            });
        }
        if (dto.IdCliente <= 0)
            return BadRequest(new { mensaje = "El cliente no es válido." });

        if (dto.IdVehiculo <= 0)
            return BadRequest(new { mensaje = "Selecciona un vehículo válido." });

        if (dto.IdTipoServicio <= 0)
            return BadRequest(new { mensaje = "Selecciona un servicio válido." });

        if (dto.FechaCita == default)
            return BadRequest(new { mensaje = "Selecciona una fecha válida." });

        var (cita, error) = await _citasService.RegistrarCitaAsync(dto);

        if (error is not null)
            return BadRequest(new { mensaje = error });

        return Ok(cita);
    }

    [HttpGet("agentes")]
    public async Task<ActionResult<List<AgenteDisponibleDto>>>
    ObtenerAgentesActivos()
    {
        return Ok(await _citasService.ObtenerAgentesActivosAsync());
    }

    [HttpGet("agentes/disponibles")]
    public async Task<ActionResult<List<AgenteDisponibleDto>>>
        ObtenerAgentesDisponibles(
            [FromQuery] DateOnly fecha,
            [FromQuery] TimeOnly horaInicio,
            [FromQuery] long idTipoServicio)
    {
        if (idTipoServicio <= 0)
            return BadRequest(new { mensaje = "El servicio no es válido." });

        if (fecha == default)
            return BadRequest(new { mensaje = "La fecha no es válida." });

        var agentes = await _citasService.ObtenerAgentesDisponiblesAsync(
            fecha, horaInicio, idTipoServicio);

        return Ok(agentes);
    }

    [HttpGet("tipos-servicio")]
    public async Task<ActionResult<List<TipoServicioDto>>> ObtenerTiposServicio()
    {
        var servicios =
            await _citasService.ObtenerTiposServicioActivosAsync();

        return Ok(servicios);
    }

}