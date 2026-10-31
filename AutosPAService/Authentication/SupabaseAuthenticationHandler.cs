
using System.Security.Claims;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Net.Http.Headers;
using AutosPAService.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AutosPAService.Authentication;

public class SupabaseAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public SupabaseAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ApplicationDbContext context)
        : base(options, logger, encoder)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();

        Logger.LogInformation(
            "Autenticación recibida. ¿Tiene Bearer?: {TieneBearer}",
            authorization.StartsWith(
                "Bearer ",
                StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith(
                "Bearer ",
                StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authorization["Bearer ".Length..].Trim();
        var supabaseUrl = _configuration["Supabase:Url"];
        var supabaseKey = _configuration["Supabase:AnonKey"];

        if (string.IsNullOrWhiteSpace(supabaseUrl) ||
            string.IsNullOrWhiteSpace(supabaseKey))
        {
            Logger.LogError("Configuración de Supabase incompleta.");
            return AuthenticateResult.Fail(
                "Configuración de Supabase incompleta.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient();

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{supabaseUrl.TrimEnd('/')}/auth/v1/user");

            request.Headers.Add("apikey", supabaseKey);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning(
                    "Supabase rechazó el token. Código HTTP: {StatusCode}",
                    (int)response.StatusCode);

                return AuthenticateResult.Fail(
                    "Token de Supabase inválido.");
            }

            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync());

            var authUserId = document.RootElement
                .GetProperty("id")
                .GetGuid();

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AuthUserId == authUserId);

            if (usuario == null)
            {
                Logger.LogWarning(
                    "El usuario autenticado de Supabase no está vinculado a AutosPA.");

                return AuthenticateResult.Fail(
                    "Usuario no vinculado a AutosPA.");
            }

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),

                new Claim(
                    "supabase_user_id",
                    authUserId.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Logger.LogInformation(
                "Autenticación exitosa para el usuario AutosPA {IdUsuario}.",
                usuario.IdUsuario);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Error validando el token de Supabase.");

            return AuthenticateResult.Fail(
                "No se pudo validar la sesión.");
        }
    }
}
