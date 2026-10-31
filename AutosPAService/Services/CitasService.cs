using AutosPAService.DTOs;
using AutosPAService.Repositories;

namespace AutosPAService.Services;

public class CitasService
{
    private readonly CitasRepository _repository;

    public CitasService(CitasRepository repository)
    {
        _repository = repository;
    }

    public Task<List<CitaDTO>> ObtenerPorClienteAsync(long idCliente)
    {
        return _repository.ObtenerPorClienteAsync(idCliente);
    }

    public Task<CitaDTO?> CrearAsync(
        long idCliente,
        CrearCitaDTO datos)
    {
        return _repository.CrearAsync(idCliente, datos);
    }

public Task<bool> CancelarAsync(long idCliente, long idCita)
{
    return _repository.CancelarAsync(idCliente, idCita);
}

}
