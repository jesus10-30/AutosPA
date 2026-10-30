using AutosPALibrary.DTOs;

namespace AutosPAService.Interfaces;

public interface IClienteRepository
{
    Task<PerfilClienteDTO?> ObtenerPerfilAsync(long idUsuario);

    Task<bool> ActualizarPerfilAsync(
        long idUsuario,
        ActualizarPerfilClienteDTO perfil);
}