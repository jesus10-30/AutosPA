using AutosPALibrary.Dtos;
using AutosPAService.Entities;

namespace AutosPAService.Services.Registro
{
    public interface IRegistroService
    {
        Task<bool> RegistrarAsync(RegistroRequest request);
    }
}
