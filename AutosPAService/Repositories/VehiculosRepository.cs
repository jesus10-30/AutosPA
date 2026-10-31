
using AutosPAService.Data;
using AutosPAService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Repositories;

public class VehiculosRepository
{
    private readonly ApplicationDbContext _context;

    public VehiculosRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Vehiculo>> ObtenerPorUsuarioAsync(long idUsuario)
    {
        return await _context.Vehiculos
            .AsNoTracking()
            .Where(v => v.IdConductorNavigation.IdUsuario == idUsuario)
            .Include(v => v.IdMarcaNavigation)
            .Include(v => v.IdTipoVehiculoNavigation)
            .OrderBy(v => v.IdMarcaNavigation.Nombre)
            .ToListAsync();
    }

    public async Task<Vehiculo?> ObtenerPorPlacaAsync(string placa)
    {
        return await _context.Vehiculos
            .FirstOrDefaultAsync(v => v.Placa == placa);
    }

    public async Task<Vehiculo> CrearAsync(Vehiculo vehiculo)
    {
        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        return vehiculo;
    }

    public async Task<bool> EliminarAsync(long idVehiculo)
    {
        var vehiculo = await _context.Vehiculos
            .FirstOrDefaultAsync(v => v.IdVehiculo == idVehiculo);

        if (vehiculo is null)
            return false;

        _context.Vehiculos.Remove(vehiculo);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<CatMarca>> ObtenerMarcasActivasAsync()
    {
        return await _context.CatMarcas
            .AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.Nombre)
            .ToListAsync();
    }

    public async Task<List<CatTiposVehiculo>> ObtenerTiposActivosAsync()
    {
        return await _context.CatTiposVehiculos
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task<long?> ObtenerIdConductorPorUsuarioAsync(long idUsuario)
{
    return await _context.Conductores
        .AsNoTracking()
        .Where(c => c.IdUsuario == idUsuario)
        .Select(c => (long?)c.IdConductor)
        .FirstOrDefaultAsync();
}

public async Task<bool> ExisteMarcaActivaAsync(long idMarca)
{
    return await _context.CatMarcas
        .AnyAsync(m => m.IdMarca == idMarca && m.Activo);
}

public async Task<bool> ExisteTipoActivoAsync(long idTipoVehiculo)
{
    return await _context.CatTiposVehiculos
        .AnyAsync(t => t.IdTipoVehiculo == idTipoVehiculo && t.Activo);
}

public async Task<bool> EliminarPorUsuarioAsync(
    long idVehiculo,
    long idUsuario)
{
    var vehiculo = await _context.Vehiculos
        .FirstOrDefaultAsync(v =>
            v.IdVehiculo == idVehiculo &&
            v.IdConductorNavigation.IdUsuario == idUsuario);

    if (vehiculo is null)
        return false;

    _context.Vehiculos.Remove(vehiculo);
    await _context.SaveChangesAsync();

    return true;
}
}
