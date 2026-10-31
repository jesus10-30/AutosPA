using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AutosPALibrary.DTOs;

namespace AutosPAService.Services;

public enum ResultadoCambioPassword
{
    Exito,
    SesionInvalida,
    PasswordActualIncorrecta,
    PasswordNuevaInvalida,
    ReautenticacionAdicionalRequerida,
    ServicioNoDisponible
}

public class AutenticacionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AutenticacionService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<ResultadoCambioPassword> CambiarPasswordAsync(
        Guid authUserId,
        string accessToken,
        CambiarPasswordDTO solicitud)
    {
        var supabaseUrl = _configuration["Supabase:Url"]?.TrimEnd('/');
        var apiKey = _configuration["Supabase:PublishableKey"] ??
                     _configuration["Supabase:AnonKey"];

        if (string.IsNullOrWhiteSpace(supabaseUrl) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(accessToken))
            return ResultadoCambioPassword.ServicioNoDisponible;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var (email, errorCorreo) = await ObtenerCorreoAutenticadoAsync(
                client, supabaseUrl, apiKey, accessToken, authUserId);

            if (errorCorreo is not null)
                return errorCorreo.Value;
            if (string.IsNullOrWhiteSpace(email))
                return ResultadoCambioPassword.ServicioNoDisponible;

            using var reautenticacion = new HttpRequestMessage(
                HttpMethod.Post,
                $"{supabaseUrl}/auth/v1/token?grant_type=password");
            reautenticacion.Headers.Add("apikey", apiKey);
            reautenticacion.Content = JsonContent.Create(new
            {
                email,
                password = solicitud.PasswordActual
            });

            using var respuestaReautenticacion = await client.SendAsync(reautenticacion);
            if (respuestaReautenticacion.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
                return ResultadoCambioPassword.PasswordActualIncorrecta;
            if (!respuestaReautenticacion.IsSuccessStatusCode)
                return ResultadoCambioPassword.ServicioNoDisponible;

            using var jsonReautenticacion = await JsonDocument.ParseAsync(
                await respuestaReautenticacion.Content.ReadAsStreamAsync());
            var raiz = jsonReautenticacion.RootElement;
            var usuarioReautenticado = raiz.GetProperty("user").GetProperty("id").GetGuid();
            var tokenReautenticado = raiz.GetProperty("access_token").GetString();

            if (usuarioReautenticado != authUserId || string.IsNullOrWhiteSpace(tokenReautenticado))
                return ResultadoCambioPassword.SesionInvalida;

            using var actualizar = new HttpRequestMessage(
                HttpMethod.Put,
                $"{supabaseUrl}/auth/v1/user");
            actualizar.Headers.Add("apikey", apiKey);
            actualizar.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenReautenticado);
            actualizar.Content = JsonContent.Create(new
            {
                password = solicitud.PasswordNueva
            });

            using var respuestaActualizar = await client.SendAsync(actualizar);
            if (respuestaActualizar.IsSuccessStatusCode)
                return ResultadoCambioPassword.Exito;
            if (respuestaActualizar.StatusCode == HttpStatusCode.Unauthorized)
                return ResultadoCambioPassword.SesionInvalida;
            if (((int)respuestaActualizar.StatusCode is 400 or 422) &&
                await ContieneCodigoAsync(respuestaActualizar, "reauthentication_needed"))
                return ResultadoCambioPassword.ReautenticacionAdicionalRequerida;
            if ((int)respuestaActualizar.StatusCode is 400 or 422)
                return ResultadoCambioPassword.PasswordNuevaInvalida;

            return ResultadoCambioPassword.ServicioNoDisponible;
        }
        catch (HttpRequestException)
        {
            return ResultadoCambioPassword.ServicioNoDisponible;
        }
        catch (TaskCanceledException)
        {
            return ResultadoCambioPassword.ServicioNoDisponible;
        }
        catch (JsonException)
        {
            return ResultadoCambioPassword.ServicioNoDisponible;
        }
    }

    private static async Task<bool> ContieneCodigoAsync(
        HttpResponseMessage response,
        string codigoEsperado)
    {
        try
        {
            using var error = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync());
            return error.RootElement.ValueKind == JsonValueKind.Object &&
                   error.RootElement.TryGetProperty("code", out var code) &&
                   string.Equals(code.GetString(), codigoEsperado, StringComparison.Ordinal);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static async Task<(string? Email, ResultadoCambioPassword? Error)> ObtenerCorreoAutenticadoAsync(
        HttpClient client,
        string supabaseUrl,
        string apiKey,
        string accessToken,
        Guid authUserId)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{supabaseUrl}/auth/v1/user");
        request.Headers.Add("apikey", apiKey);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return (null, ResultadoCambioPassword.SesionInvalida);
        if (!response.IsSuccessStatusCode)
            return (null, ResultadoCambioPassword.ServicioNoDisponible);

        using var user = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        var root = user.RootElement;
        if (!Guid.TryParse(root.GetProperty("id").GetString(), out var userId) ||
            userId != authUserId)
            return (null, ResultadoCambioPassword.SesionInvalida);
        if (!root.TryGetProperty("email", out var email))
            return (null, ResultadoCambioPassword.ServicioNoDisponible);

        return (email.GetString(), null);
    }
}
