
using AutosPAService.Data;
using AutosPAService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CatalogosController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("marcas")]
    public async Task<IActionResult> ObtenerMarcas()
    {
        var marcas = await _context.Set<CatMarca>()
            .AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.Nombre)
            .Select(m => new
            {
                id = m.IdMarca,
                nombre = m.Nombre
            })
            .ToListAsync();

        return Ok(marcas);
    }

    [HttpGet("tipos-vehiculo")]
    public async Task<IActionResult> ObtenerTiposVehiculo()
    {
        var tipos = await _context.Set<CatTiposVehiculo>()
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .Select(t => new
            {
                id = t.IdTipoVehiculo,
                nombre = t.Nombre
            })
            .ToListAsync();

        return Ok(tipos);
    }
}
