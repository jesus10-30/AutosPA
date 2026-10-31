using AutosPAService.Interfaces;
using AutosPALibrary.DTOs;

namespace AutosPAService.Services;

public class ClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> EsClienteAsync(long idUsuario)
    {
        return _repository.EsClienteAsync(idUsuario);
    }

    public Task<PerfilClienteDTO?> ObtenerPerfilAsync(long idUsuario)
    {
        return _repository.ObtenerPerfilAsync(idUsuario);
    }

    public async Task<ResultadoActualizacionPerfil> ActualizarPerfilAsync(
        long idUsuario,
        ActualizarPerfilClienteDTO perfil)
    {
        var perfilActual = await _repository.ObtenerPerfilAsync(idUsuario);
        if (perfilActual is null)
            return ResultadoActualizacionPerfil.UsuarioNoEncontrado;

        if (!string.Equals(perfilActual.Email.Trim(), perfil.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            if (await _repository.CorreoEnUsoAsync(idUsuario, perfil.Email))
                return ResultadoActualizacionPerfil.CorreoDuplicado;

            return ResultadoActualizacionPerfil.CambioCorreoRequiereSincronizacionAuth;
        }

        var actualizado = await _repository.ActualizarPerfilAsync(idUsuario, perfil);
        return actualizado
            ? ResultadoActualizacionPerfil.Exito
            : ResultadoActualizacionPerfil.UsuarioNoEncontrado;
    }
}
