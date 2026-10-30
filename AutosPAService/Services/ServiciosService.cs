
using AutosPALibrary.DTOs;
using AutosPAService.Repositories;

namespace AutosPAService.Services;

public class ServiciosService
{
    private readonly ServiciosRepository _repository;

    public ServiciosService(ServiciosRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ServicioDTO>> ObtenerServiciosActivosAsync()
    {
        var servicios = await _repository.ObtenerServiciosActivosAsync();

        return servicios.Select(servicio => new ServicioDTO
        {
            Id = servicio.IdTipoServicio,
            Nombre = servicio.Nombre,
            Descripcion = servicio.Descripcion,
            CostoBase = servicio.CostoBase,
            TiempoEstimadoMinutos = servicio.TiempoEstimadoMinutos
        }).ToList();
    }
}
