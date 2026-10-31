using AutosPALibrary.DTOs;

namespace AutosPAService.Interfaces;

public interface INotificacionesService
{
    Task<List<NotificacionDTO>> ObtenerPorClienteAsync(long idCliente);

    Task<bool> MarcarComoLeidaAsync(long idNotificacion, long idCliente);

    Task<int> MarcarTodasComoLeidasAsync(long idCliente);

    Task<bool> EliminarPorClienteAsync(long idNotificacion, long idCliente);
}
