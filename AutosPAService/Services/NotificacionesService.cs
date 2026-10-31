
using AutosPALibrary.DTOs;
using AutosPAService.Interfaces;

namespace AutosPAService.Services;

public class NotificacionesService : INotificacionesService
{
    private readonly INotificacionesRepository _repository;

    public NotificacionesService(INotificacionesRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NotificacionDTO>> ObtenerPorClienteAsync(long idCliente)
    {
        return await _repository.ObtenerPorClienteAsync(idCliente);
    }

    public async Task<bool> MarcarComoLeidaAsync(
        long idNotificacion,
        long idCliente)
    {
        return await _repository.MarcarComoLeidaAsync(
            idNotificacion, idCliente);
    }

    public async Task<int> MarcarTodasComoLeidasAsync(long idCliente)
    {
        return await _repository.MarcarTodasComoLeidasAsync(idCliente);
    }

    public async Task<bool> EliminarPorClienteAsync(
        long idNotificacion,
        long idCliente)
    {
        return await _repository.EliminarPorClienteAsync(
            idNotificacion, idCliente);
    }
}
