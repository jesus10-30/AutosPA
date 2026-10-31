
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace AutosPA.Services;

public class SessionAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;

    public SessionAuthenticationStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _js.InvokeAsync<string?>(
                "sessionStorage.getItem",
                "flexicar_access_token");

            var rol = await _js.InvokeAsync<string?>(
                "sessionStorage.getItem",
                "flexicar_rol");

            if (string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(rol))
            {
                return EstadoAnonimo();
            }

            var nombres = await _js.InvokeAsync<string?>(
                "sessionStorage.getItem",
                "flexicar_nombres");

            var apellidos = await _js.InvokeAsync<string?>(
                "sessionStorage.getItem",
                "flexicar_apellidos");

            var nombreCompleto =
                $"{nombres} {apellidos}".Trim();

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.Name,
                    string.IsNullOrWhiteSpace(nombreCompleto)
                        ? "Usuario"
                        : nombreCompleto),

                new Claim(
                    ClaimTypes.Role,
                    rol.Trim().ToUpperInvariant())
            };

            var identidad = new ClaimsIdentity(
                claims,
                authenticationType: "FlexiCar");

            var usuario = new ClaimsPrincipal(identidad);

            return new AuthenticationState(usuario);
        }
        catch (JSException)
        {
            return EstadoAnonimo();
        }
        catch (InvalidOperationException)
        {
            return EstadoAnonimo();
        }
    }

    public async Task ActualizarEstadoAsync()
    {
        var estado = await GetAuthenticationStateAsync();

        NotifyAuthenticationStateChanged(
            Task.FromResult(estado));
    }

    private static AuthenticationState EstadoAnonimo()
    {
        return new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity()));
    }
}
