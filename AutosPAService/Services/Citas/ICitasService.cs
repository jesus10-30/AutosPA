
using AutosPALibrary.Dtos;

namespace AutosPAService.Services.Citas;

public interface ICitasService
{
    Task<List<CitaClienteDto>> ObtenerCitasPorClienteAsync(long idCliente);

    Task<List<VehiculoClienteDto>> ObtenerVehiculosPorClienteAsync(long idCliente);

    Task<List<OpcionCatalogoDto>> ObtenerMarcasActivasAsync();

    Task<List<OpcionCatalogoDto>> ObtenerTiposVehiculoActivosAsync();

    Task<(VehiculoClienteDto? Vehiculo, string? Error)> RegistrarVehiculoAsync(
        RegistrarVehiculoDto dto);

    Task<(CitaClienteDto? Cita, string? Error)>
    RegistrarCitaAsync(RegistrarCitaDto dto);

    Task<List<AgenteDisponibleDto>> ObtenerAgentesActivosAsync();

    Task<List<AgenteDisponibleDto>> ObtenerAgentesDisponiblesAsync(
        DateOnly fecha,
        TimeOnly horaInicio,
        long idTipoServicio);

    Task<List<TipoServicioDto>> ObtenerTiposServicioActivosAsync();
}


