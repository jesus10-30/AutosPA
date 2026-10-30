using AutosPAService.Interfaces;
using AutosPALibrary.DTOs;

namespace AutosPAService.Services;

public class AgenteService
{
    private readonly IAgenteRepository _repository;

    public AgenteService(IAgenteRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> EsAgenteAsync(long idUsuario)
    {
        return _repository.EsAgenteAsync(idUsuario);
    }

    public Task<List<AgenteTrabajoDTO>> ObtenerTrabajosAsync(long idAgente)
    {
        return _repository.ObtenerTrabajosAsync(idAgente);
    }

    public Task<bool> IniciarTrabajoAsync(long idCita, long idAgente)
    {
        return _repository.IniciarTrabajoAsync(idCita, idAgente);
    }

    public Task<bool> FinalizarTrabajoAsync(long idCita, long idAgente)
    {
        return _repository.FinalizarTrabajoAsync(idCita, idAgente);
    }

    public Task<PerfilAgenteDTO?> ObtenerPerfilAsync(long idUsuario)
    {
        return _repository.ObtenerPerfilAsync(idUsuario);
    }

    public Task<bool> ActualizarPerfilAsync(
        long idUsuario,
        ActualizarPerfilAgenteDTO perfil)
    {
        return _repository.ActualizarPerfilAsync(idUsuario, perfil);
    }
}