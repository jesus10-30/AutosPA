using AutosPAService.Data;
using AutosPAService.Interfaces;
using AutosPALibrary.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Repositories;

public class AgenteRepository : IAgenteRepository
{
    private readonly ApplicationDbContext _context;

    public AgenteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EsAgenteAsync(long idUsuario)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        return usuario != null &&
               usuario.IdRolNavigation.Nombre.Equals(
                   "agente",
                   StringComparison.OrdinalIgnoreCase);
    }

    public async Task<List<AgenteTrabajoDTO>> ObtenerTrabajosAsync(long idAgente)
    {
        return await _context.Citas
            .AsNoTracking()
            .Where(c => c.IdAgente == idAgente)
            .OrderBy(c => c.FechaCita)
            .ThenBy(c => c.HoraInicio)
            .Select(c => new AgenteTrabajoDTO
            {
                IdCita = c.IdCita,
                FechaCita = c.FechaCita,
                HoraInicio = c.HoraInicio,
                Cliente = c.IdClienteNavigation.Nombres + " " +
                          c.IdClienteNavigation.Apellidos,
                Vehiculo = c.IdVehiculoNavigation.IdMarcaNavigation.Nombre + " " +
                           c.IdVehiculoNavigation.Modelo + " " +
                           c.IdVehiculoNavigation.Anio,
                Placa = c.IdVehiculoNavigation.Placa,
                Servicio = c.IdTipoServicioNavigation.Nombre,
                Estado = c.IdEstadoCitaNavigation.Nombre,
                Costo = c.Costo,
                TiempoEstimadoMinutos = c.TiempoEstimadoMinutos,
                Observaciones = c.Observaciones
            })
            .ToListAsync();
    }

    public async Task<bool> IniciarTrabajoAsync(long idCita, long idAgente)
    {
        var cita = await _context.Citas.FirstOrDefaultAsync(c =>
            c.IdCita == idCita &&
            c.IdAgente == idAgente &&
            c.IdEstadoCita == 2);

        if (cita == null)
            return false;

        cita.IdEstadoCita = 3;
        cita.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> FinalizarTrabajoAsync(long idCita, long idAgente)
    {
        var cita = await _context.Citas.FirstOrDefaultAsync(c =>
            c.IdCita == idCita &&
            c.IdAgente == idAgente &&
            c.IdEstadoCita == 3);

        if (cita == null)
            return false;

        cita.IdEstadoCita = 4;
        cita.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PerfilAgenteDTO?> ObtenerPerfilAsync(long idUsuario)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => new PerfilAgenteDTO
            {
                Nombres = u.Nombres,
                Apellidos = u.Apellidos,
                Email = u.Email,
                Telefono = u.Telefono
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ActualizarPerfilAsync(
        long idUsuario,
        ActualizarPerfilAgenteDTO perfil)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            return false;

        usuario.Nombres = perfil.Nombres.Trim();
        usuario.Apellidos = perfil.Apellidos.Trim();
        usuario.Email = perfil.Email.Trim();
        usuario.Telefono = string.IsNullOrWhiteSpace(perfil.Telefono)
            ? null
            : perfil.Telefono.Trim();

        usuario.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}