using AutosPALibrary.Dtos;
using AutosPAService.Data;
using AutosPAService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Repositories
{
    public class AuthRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorAuthUserIdAsync(Guid authUserId)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.IdRolNavigation)
                .Include(u => u.IdEstadoUsuarioNavigation)
                .FirstOrDefaultAsync(u => u.AuthUserId == authUserId);
        }


        public async Task<Usuario?> ObtenerPorIdAsync(long idUsuario)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }


        public async Task<bool> ActualizarPerfilAsync(
            long idUsuario,
            string nombres,
            string apellidos,
            string? telefono)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario is null)
                return false;

            usuario.Nombres = nombres;
            usuario.Apellidos = apellidos;
            usuario.Telefono = telefono;
            usuario.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<Usuario?> ObtenerUsuarioParaCambiarPasswordAsync(
            long idUsuario)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }



        public async Task<EstadisticasCuentaDto?> ObtenerEstadisticasCuentaAsync(
            long idUsuario)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Conductore)
                    .ThenInclude(c => c!.Vehiculos)
                .Include(u => u.CitaIdClienteNavigations)
                    .ThenInclude(c => c.IdEstadoCitaNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario is null)
                return null;

            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var citas = usuario.CitaIdClienteNavigations;

            return new EstadisticasCuentaDto
            {
                VehiculosRegistrados =
                    usuario.Conductore?.Vehiculos.Count ?? 0,

                CitasProximas =
                    citas.Count(c =>
                        c.FechaCita >= hoy &&
                        (c.IdEstadoCitaNavigation.Nombre == "PENDIENTE" ||
                         c.IdEstadoCitaNavigation.Nombre == "CONFIRMADA")),

                ServiciosCompletados =
                    citas.Count(c =>
                        c.IdEstadoCitaNavigation.Nombre == "FINALIZADA")
            };
        }




    }
}
