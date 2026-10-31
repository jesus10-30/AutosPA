using AutosPAService.Interfaces;
using AutosPALibrary.DTOs;

namespace AutosPAService.Services;

public class ClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public Task<PerfilClienteDTO?> ObtenerPerfilAsync(long idUsuario)
    {
        return _repository.ObtenerPerfilAsync(idUsuario);
    }

    public Task<bool> ActualizarPerfilAsync(
        long idUsuario,
        ActualizarPerfilClienteDTO perfil)
    {
        return _repository.ActualizarPerfilAsync(idUsuario, perfil);
    }
}