
using AutosPAService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutosPAService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiciosController : ControllerBase
{
    private readonly ServiciosService _serviciosService;

    public ServiciosController(ServiciosService serviciosService)
    {
        _serviciosService = serviciosService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerServicios()
    {
        var servicios = await _serviciosService.ObtenerServiciosActivosAsync();

        return Ok(servicios);
    }
}
