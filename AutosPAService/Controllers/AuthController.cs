using System.Net.Http.Json;
using System.Text.Json;
using AutosPAService.Data;
using AutosPAService.Services;
using AutosPALibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AutenticacionService _autenticacionService;

        public AuthController(
            ApplicationDbContext context,
            HttpClient httpClient,
            IConfiguration configuration,
            AutenticacionService autenticacionService)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
            _autenticacionService = autenticacionService;
        }

        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> CambiarPassword(
            [FromBody] CambiarPasswordDTO solicitud)
        {
            if (!Guid.TryParse(
                    User.FindFirst("supabase_user_id")?.Value,
                    out var authUserId))
                return Unauthorized(new { mensaje = "La sesión no es válida." });

            if (string.IsNullOrEmpty(solicitud.PasswordActual) ||
                string.IsNullOrEmpty(solicitud.PasswordNueva) ||
                solicitud.PasswordNueva.Length < 8)
                return BadRequest(new { mensaje = "La nueva contraseña debe tener al menos 8 caracteres." });

            if (string.Equals(solicitud.PasswordActual, solicitud.PasswordNueva, StringComparison.Ordinal))
                return BadRequest(new { mensaje = "La contraseña nueva debe ser diferente a la actual." });

            var token = Request.Headers.Authorization.ToString();
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token["Bearer ".Length..].Trim();

            var resultado = await _autenticacionService.CambiarPasswordAsync(
                authUserId,
                token,
                solicitud);

            return resultado switch
            {
                ResultadoCambioPassword.Exito => NoContent(),
                ResultadoCambioPassword.SesionInvalida =>
                    Unauthorized(new { mensaje = "La sesión venció. Inicia sesión nuevamente." }),
                ResultadoCambioPassword.PasswordActualIncorrecta =>
                    BadRequest(new { mensaje = "La contraseña actual no es correcta." }),
                ResultadoCambioPassword.PasswordNuevaInvalida =>
                    BadRequest(new { mensaje = "Supabase Auth rechazó la nueva contraseña. Revisa los requisitos configurados." }),
                ResultadoCambioPassword.ReautenticacionAdicionalRequerida =>
                    Conflict(new { mensaje = "Supabase Auth requiere un código adicional para confirmar el cambio. Este flujo todavía no está configurado." }),
                _ => StatusCode(StatusCodes.Status502BadGateway,
                    new { mensaje = "No se pudo completar el cambio de contraseña con Supabase Auth." })
            };
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var supabaseUrl = _configuration["Supabase:Url"];
            var supabaseKey = _configuration["Supabase:PublishableKey"];

            if (string.IsNullOrWhiteSpace(supabaseUrl) ||
                string.IsNullOrWhiteSpace(supabaseKey))
            {
                return StatusCode(500, new
                {
                    mensaje = "La configuración de Supabase está incompleta."
                });
            }

            var url = $"{supabaseUrl.TrimEnd('/')}/auth/v1/token?grant_type=password";

            using var mensaje = new HttpRequestMessage(HttpMethod.Post, url);

            mensaje.Headers.Add("apikey", supabaseKey);

            mensaje.Content = JsonContent.Create(new
            {
                email = request.Correo,
                password = request.Password
            });

            using var respuesta = await _httpClient.SendAsync(mensaje);

            if (!respuesta.IsSuccessStatusCode)
            {
                return Unauthorized(new
                {
                    mensaje = "Correo o contraseña incorrectos."
                });
            }

            using var contenido = await JsonDocument.ParseAsync(
                await respuesta.Content.ReadAsStreamAsync()
            );

            var token = contenido.RootElement
                .GetProperty("access_token")
                .GetString();

            var authUserId = contenido.RootElement
                .GetProperty("user")
                .GetProperty("id")
                .GetGuid();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.AuthUserId == authUserId);

            if (usuario == null)
            {
                return StatusCode(403, new
                {
                    mensaje = "La cuenta existe en Supabase, pero no está vinculada a un usuario de AutosPA."
                });
            }

            return Ok(new
            {
                token = token,
                idUsuario = usuario.IdUsuario,
                idRol = usuario.IdRol
            });
        }
    }

    public class LoginRequest
    {
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
