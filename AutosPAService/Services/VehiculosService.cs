using AutosPAService.DTOs;
using AutosPAService.Entities;
using AutosPAService.Repositories;

namespace AutosPAService.Services;

public class VehiculosService
{
    private readonly VehiculosRepository _repository;

    public VehiculosService(VehiculosRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<VehiculoDTO>> ObtenerPorUsuarioAsync(long idUsuario)
    {
        var vehiculos = await _repository.ObtenerPorUsuarioAsync(idUsuario);

        return vehiculos.Select(v => new VehiculoDTO
        {
            IdVehiculo = v.IdVehiculo,
            Marca = v.IdMarcaNavigation.Nombre,
            Modelo = v.Modelo,
            Anio = v.Anio,
            Placa = v.Placa,
            Tipo = v.IdTipoVehiculoNavigation.Nombre,
            Color = v.Color,
            Kilometraje = v.Kilometraje
        }).ToList();
    }

    public async Task<(bool Exito, string Mensaje)> CrearAsync(
        long idUsuario,
        CrearVehiculoDTO dto)
    {
        var idConductor = await _repository.ObtenerIdConductorPorUsuarioAsync(idUsuario);

        if (idConductor is null)
            return (false, "El usuario no tiene un conductor registrado.");

        if (!await _repository.ExisteMarcaActivaAsync(dto.IdMarca))
            return (false, "La marca seleccionada no existe o estÃ¡ inactiva.");

        if (!await _repository.ExisteTipoActivoAsync(dto.IdTipoVehiculo))
            return (false, "El tipo de vehÃ­culo seleccionado no existe o estÃ¡ inactivo.");

        var placa = dto.Placa.Trim().ToUpperInvariant();

        if (await _repository.ObtenerPorPlacaAsync(placa) is not null)
            return (false, "Ya existe un vehÃ­culo registrado con esa placa.");

        var vehiculo = new Vehiculo
        {
            IdConductor = idConductor.Value,
            IdMarca = dto.IdMarca,
            IdTipoVehiculo = dto.IdTipoVehiculo,
            Modelo = dto.Modelo.Trim(),
            Anio = dto.Anio,
            Placa = placa,
            Color = dto.Color?.Trim(),
            Kilometraje = dto.Kilometraje
        };

        await _repository.CrearAsync(vehiculo);

        return (true, "VehÃ­culo registrado correctamente.");
    }

    public Task<Vehiculo?> ObtenerPorPlacaAsync(string placa)
    {
        return _repository.ObtenerPorPlacaAsync(placa);
    }

    public Task<bool> EliminarPorUsuarioAsync(long idVehiculo, long idUsuario)
    {
        return _repository.EliminarPorUsuarioAsync(idVehiculo, idUsuario);
    }

    public Task<List<CatMarca>> ObtenerMarcasActivasAsync()
    {
        return _repository.ObtenerMarcasActivasAsync();
    }

    public Task<List<CatTiposVehiculo>> ObtenerTiposActivosAsync()
    {
        return _repository.ObtenerTiposActivosAsync();
    }
}


