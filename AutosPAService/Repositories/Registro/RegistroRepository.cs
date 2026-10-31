
using AutosPAService.Data;
using AutosPAService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Repositories.Registro;

public class RegistroRepository : IRegistroRepository
{
    private readonly ApplicationDbContext _context;

    public RegistroRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Usuario> CrearUsuarioAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return usuario;
    }

    public async Task<Conductore> CrearConductorAsync(
        Conductore conductor)
    {
        _context.Conductores.Add(conductor);
        await _context.SaveChangesAsync();

        return conductor;
    }

    public async Task<Vehiculo> CrearVehiculoAsync(
        Vehiculo vehiculo)
    {
        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        return vehiculo;
    }
}
