using AutosPALibrary.DTOs;

namespace AutosPAService.Interfaces;

public interface IAgenteRepository
{
    Task<List<AgenteTrabajoDTO>> ObtenerTrabajosAsync(long idAgente);
    Task<bool> EsAgenteAsync(long idUsuario);
    Task<bool> IniciarTrabajoAsync(long idCita, long idAgente);
    Task<bool> FinalizarTrabajoAsync(long idCita, long idAgente);

    Task<PerfilAgenteDTO?> ObtenerPerfilAsync(long idUsuario);
    Task<bool> ActualizarPerfilAsync(long idUsuario, ActualizarPerfilAgenteDTO perfil);
}