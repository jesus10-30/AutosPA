using AutosPAService.Data;
using AutosPAService.DTOs;
using AutosPAService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Repositories;

public class CitasRepository
{
    private readonly ApplicationDbContext _context;

    public CitasRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CitaDTO>> ObtenerPorClienteAsync(long idCliente)
    {
        return await _context.Citas
            .AsNoTracking()
            .Where(c => c.IdCliente == idCliente)
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraInicio)
            .Select(c => new CitaDTO
            {
                IdCita = c.IdCita,
                IdVehiculo = c.IdVehiculo,
                Placa = c.IdVehiculoNavigation.Placa,
                ModeloVehiculo = c.IdVehiculoNavigation.Modelo,
                Servicio = c.IdTipoServicioNavigation.Nombre,
                Estado = c.IdEstadoCitaNavigation.Nombre,
                FechaCita = c.FechaCita,
                HoraInicio = c.HoraInicio,
                Costo = c.Costo,
                TiempoEstimadoMinutos = c.TiempoEstimadoMinutos,
                Observaciones = c.Observaciones
            })
            .ToListAsync();
    }

    public async Task<CitaDTO?> CrearAsync(
        long idCliente,
        CrearCitaDTO datos)
    {
        // Comprobar que el vehículo pertenezca al cliente.
        var vehiculo = await _context.Vehiculos
            .FirstOrDefaultAsync(v =>
                v.IdVehiculo == datos.IdVehiculo &&
                v.IdConductorNavigation.IdUsuario == idCliente);

        if (vehiculo is null)
            return null;

        // Obtener el servicio desde la base de datos.
        var servicio = await _context.CatTiposServicios
            .FirstOrDefaultAsync(s =>
                s.IdTipoServicio == datos.IdTipoServicio &&
                s.Activo);

        if (servicio is null)
            return null;

        // Crear la cita con estado PENDIENTE.
        var cita = new Cita
        {
            IdCliente = idCliente,
            IdVehiculo = datos.IdVehiculo,
            IdTipoServicio = datos.IdTipoServicio,
            IdEstadoCita = 1,
            FechaCita = datos.FechaCita,
            HoraInicio = datos.HoraInicio,
            Costo = servicio.CostoBase,
            TiempoEstimadoMinutos = servicio.TiempoEstimadoMinutos,
            Observaciones = datos.Observaciones
        };

        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();

        // Devolver la cita recién creada con sus datos relacionados.
        return await _context.Citas
            .AsNoTracking()
            .Where(c => c.IdCita == cita.IdCita)
            .Select(c => new CitaDTO
            {
                IdCita = c.IdCita,
                IdVehiculo = c.IdVehiculo,
                Placa = c.IdVehiculoNavigation.Placa,
                ModeloVehiculo = c.IdVehiculoNavigation.Modelo,
                Servicio = c.IdTipoServicioNavigation.Nombre,
                Estado = c.IdEstadoCitaNavigation.Nombre,
                FechaCita = c.FechaCita,
                HoraInicio = c.HoraInicio,
                Costo = c.Costo,
                TiempoEstimadoMinutos = c.TiempoEstimadoMinutos,
                Observaciones = c.Observaciones
            })
            .FirstAsync();
    }

public async Task<bool> CancelarAsync(long idCliente, long idCita)
{
    var cita = await _context.Citas
        .FirstOrDefaultAsync(c =>
            c.IdCita == idCita &&
            c.IdCliente == idCliente);

    if (cita is null)
        return false;

    // No permitir cancelar una cita ya finalizada o cancelada.
    if (cita.IdEstadoCita == 4 || cita.IdEstadoCita == 5)
        return false;

    // Estado 5 = CANCELADA.
    cita.IdEstadoCita = 5;

    await _context.SaveChangesAsync();

    return true;
}

}
