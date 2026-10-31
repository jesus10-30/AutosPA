using AutosPAService.Data;
using AutosPAService.Interfaces;
using AutosPALibrary.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _context;

    public ClienteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PerfilClienteDTO?> ObtenerPerfilAsync(long idUsuario)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            return null;

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var totalVehiculos = await _context.Vehiculos
            .CountAsync(v => v.IdConductorNavigation != null &&
                             v.IdConductorNavigation.IdUsuario == idUsuario);

        var citasProximas = await _context.Citas
            .CountAsync(c => c.IdCliente == idUsuario &&
                             c.FechaCita >= hoy &&
                             c.IdEstadoCitaNavigation.Nombre != "CANCELADA" &&
                             c.IdEstadoCitaNavigation.Nombre != "FINALIZADA");

        var serviciosCompletados = await _context.Citas
            .CountAsync(c => c.IdCliente == idUsuario &&
                             c.IdEstadoCitaNavigation.Nombre == "FINALIZADA");

        return new PerfilClienteDTO
        {
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            Email = usuario.Email,
            Telefono = usuario.Telefono,
            TotalVehiculos = totalVehiculos,
            CitasProximas = citasProximas,
            ServiciosCompletados = serviciosCompletados
        };
    }

    public async Task<bool> ActualizarPerfilAsync(
        long idUsuario,
        ActualizarPerfilClienteDTO perfil)
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