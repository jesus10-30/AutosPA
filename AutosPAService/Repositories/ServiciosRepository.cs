
using AutosPAService.Data;
using AutosPAService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Repositories;

public class ServiciosRepository
{
    private readonly ApplicationDbContext _context;

    public ServiciosRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CatTiposServicio>> ObtenerServiciosActivosAsync()
    {
        return await _context.CatTiposServicios
            .AsNoTracking()
            .Where(servicio => servicio.Activo)
            .OrderBy(servicio => servicio.Nombre)
            .ToListAsync();
    }
}
