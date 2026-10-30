using System.Net.Http.Json;
using System.Text.Json;
using AutosPAService.Data;
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

        public AuthController(
            ApplicationDbContext context,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
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