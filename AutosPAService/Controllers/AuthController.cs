using AutosPALibrary.Dtos;
using AutosPAService.Services;
using AutosPAService.Services.Registro;
using Microsoft.AspNetCore.Mvc;

namespace AutosPAService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{

    private readonly AuthService _authService;
    private readonly IRegistroService _registroService;

    public AuthController(
        AuthService authService,
        IRegistroService registroService)
    {
        _authService = authService;
        _registroService = registroService;
    }


    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request)
    {
        var resultado =
            await _authService.IniciarSesionAsync(request);

        if (resultado is null)
        {
            return Unauthorized(new
            {
                mensaje = "Credenciales incorrectas o cuenta no habilitada."
            });
        }

        return Ok(resultado);
    }

    [HttpGet("perfil/{idUsuario:long}")]
    public async Task<ActionResult<PerfilUsuarioDto>> ObtenerPerfil(long idUsuario)
    {
        if (idUsuario <= 0)
        {
            return BadRequest(new
            {
                mensaje = "El ID del usuario no es válido."
            });
        }

        var perfil = await _authService.ObtenerPerfilAsync(idUsuario);

        if (perfil is null)
        {
            return NotFound(new
            {
                mensaje = "No se encontró el usuario."
            });
        }

        return Ok(perfil);
    }


    [HttpPut("perfil/{idUsuario:long}")]
    public async Task<IActionResult> ActualizarPerfil(
        long idUsuario,
        [FromBody] ActualizarPerfilRequest request)
    {
        if (idUsuario <= 0)
        {
            return BadRequest(new
            {
                mensaje = "El ID del usuario no es válido."
            });
        }

        try
        {
            var actualizado = await _authService.ActualizarPerfilAsync(
                idUsuario,
                request);

            if (!actualizado)
            {
                return NotFound(new
                {
                    mensaje = "No se encontró el usuario."
                });
            }

            return Ok(new
            {
                mensaje = "El perfil se actualizó correctamente."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }



    [HttpPost("registro")]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistroRequest request)
    {
        try
        {
            var registrado = await _registroService.RegistrarAsync(request);

            if (!registrado)
            {
                return Conflict(new
                {
                    mensaje = "No se pudo completar el registro. Verifica si el correo ya está registrado."
                });
            }

            return Ok(new
            {
                mensaje = "Tu cuenta de FlexiCar se creó correctamente."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    [HttpPut("cambiar-password")]
    public async Task<IActionResult> CambiarPassword(
        [FromBody] CambiarPasswordRequest request)
    {
        try
        {
            var cambiado = await _authService.CambiarPasswordAsync(request);

            if (!cambiado)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo cambiar la contraseña. Verifica tu contraseña actual."
                });
            }

            return Ok(new
            {
                mensaje = "La contraseña se actualizó correctamente."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }


    [HttpGet("estadisticas/{idUsuario:long}")]
    public async Task<ActionResult<EstadisticasCuentaDto>> ObtenerEstadisticasCuenta(
        long idUsuario)
    {
        if (idUsuario <= 0)
        {
            return BadRequest(new
            {
                mensaje = "El ID del usuario no es válido."
            });
        }

        var estadisticas =
            await _authService.ObtenerEstadisticasCuentaAsync(idUsuario);

        if (estadisticas is null)
        {
            return NotFound(new
            {
                mensaje = "No se encontró el usuario."
            });
        }

        return Ok(estadisticas);
    }



}
