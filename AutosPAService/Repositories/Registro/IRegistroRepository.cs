using AutosPAService.Entities;

namespace AutosPAService.Repositories.Registro;

public interface IRegistroRepository
{
    Task<Usuario?> ObtenerPorEmailAsync(string email);

    Task<Usuario> CrearUsuarioAsync(Usuario usuario);

    Task<Conductore> CrearConductorAsync(Conductore conductor);

    Task<Vehiculo> CrearVehiculoAsync(Vehiculo vehiculo);
}
