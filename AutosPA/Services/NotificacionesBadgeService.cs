using System.Net.Http.Json;
using AutosPALibrary.DTOs;

namespace AutosPA.Services;

public class NotificacionesBadgeService
{
    private readonly HttpClient _http;

    public event Action? OnChange;

    public int CantidadSinLeer { get; private set; }

    public NotificacionesBadgeService(HttpClient http)
    {
        _http = http;
    }

    public async Task CargarAsync()
    {
        try
        {
            var datos = await _http.GetFromJsonAsync<List<NotificacionDTO>>(
                "api/Notificaciones");

            CantidadSinLeer = (datos ?? new List<NotificacionDTO>())
                .Count(n => !n.Leida);
        }
        catch (HttpRequestException)
        {
            CantidadSinLeer = 0;
        }
        catch (System.Text.Json.JsonException)
        {
            CantidadSinLeer = 0;
        }

        OnChange?.Invoke();
    }

    public void Actualizar(int cantidad)
    {
        CantidadSinLeer = Math.Max(0, cantidad);
        OnChange?.Invoke();
    }

    public void Limpiar()
    {
        CantidadSinLeer = 0;
        OnChange?.Invoke();
    }
}