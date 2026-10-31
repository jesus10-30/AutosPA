using AutosPALibrary.DTOs;

namespace AutosPAService.Interfaces;

public interface IClienteRepository
{
    Task<bool> EsClienteAsync(long idUsuario);

    Task<PerfilClienteDTO?> ObtenerPerfilAsync(long idUsuario);

    Task<bool> ActualizarPerfilAsync(
        long idUsuario,
        ActualizarPerfilClienteDTO perfil);

    Task<bool> CorreoEnUsoAsync(long idUsuario, string email);
}
