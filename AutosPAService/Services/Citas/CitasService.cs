using AutosPAService.Repositories.Citas;
using AutosPALibrary.Dtos;

namespace AutosPAService.Services.Citas;

public class CitasService : ICitasService
{
    private readonly ICitasRepository _citasRepository;

    public CitasService(ICitasRepository citasRepository)
    {
        _citasRepository = citasRepository;
    }

    public async Task<List<CitaClienteDto>> ObtenerCitasPorClienteAsync(
        long idCliente)
    {
        return await _citasRepository.ObtenerCitasPorClienteAsync(idCliente);
    }

    public async Task<List<VehiculoClienteDto>> ObtenerVehiculosPorClienteAsync(
    long idCliente)
    {
        return await _citasRepository.ObtenerVehiculosPorClienteAsync(idCliente);
    }

    public async Task<List<OpcionCatalogoDto>> ObtenerMarcasActivasAsync()
    {
        return await _citasRepository.ObtenerMarcasActivasAsync();
    }

    public async Task<List<OpcionCatalogoDto>> ObtenerTiposVehiculoActivosAsync()
    {
        return await _citasRepository.ObtenerTiposVehiculoActivosAsync();
    }

    public async Task<(VehiculoClienteDto? Vehiculo, string? Error)>
        RegistrarVehiculoAsync(RegistrarVehiculoDto dto)
    {
        return await _citasRepository.RegistrarVehiculoAsync(dto);
    }

    public Task<(CitaClienteDto? Cita, string? Error)>
    RegistrarCitaAsync(RegistrarCitaDto dto)
    {
        return _citasRepository.RegistrarCitaAsync(dto);
    }

    public Task<List<AgenteDisponibleDto>> ObtenerAgentesActivosAsync()
    {
        return _citasRepository.ObtenerAgentesActivosAsync();
    }

    public Task<List<AgenteDisponibleDto>> ObtenerAgentesDisponiblesAsync(
        DateOnly fecha,
        TimeOnly horaInicio,
        long idTipoServicio)
    {
        return _citasRepository.ObtenerAgentesDisponiblesAsync(
            fecha, horaInicio, idTipoServicio);
    }
    public Task<List<TipoServicioDto>> ObtenerTiposServicioActivosAsync()
    {
        return _citasRepository.ObtenerTiposServicioActivosAsync();
    }

}