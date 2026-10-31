using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutosPAService.Data;
using AutosPAService.DTOs;
using AutosPAService.Entities;
using AutosPAService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Services;

public class DocumentosVehiculoService
{
    private const string Bucket = "documentos-vehiculos";

    private readonly DocumentosVehiculoRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public DocumentosVehiculoService(
        DocumentosVehiculoRepository repository,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _context = context;
    }

    public Task<List<CatTiposDocumento>> ObtenerTiposAsync()
        => _repository.ObtenerTiposAsync();

    public async Task<List<DocumentoVehiculoDTO>?> ObtenerPorVehiculoAsync(
        long idVehiculo, long idUsuario)
    {
        if (!await _repository.VehiculoPerteneceAUsuarioAsync(
            idVehiculo, idUsuario))
            return null;

        return await _repository.ObtenerPorVehiculoAsync(idVehiculo);
    }

    public async Task<(bool Exito, string Mensaje)> SubirAsync(
        long idVehiculo,
        long idUsuario,
        Guid authUserId,
        long idTipoDocumento,
        DateTime? fechaVencimiento,
        IFormFile archivo,
        string token)
    {
        if (!await _repository.VehiculoPerteneceAUsuarioAsync(
            idVehiculo, idUsuario))
            return (false, "No se encontró el vehículo.");

        if (!await _repository.TipoActivoAsync(idTipoDocumento))
            return (false, "El tipo de documento no es válido.");

        if (archivo == null || archivo.Length == 0)
            return (false, "Selecciona un archivo.");

        if (archivo.Length > 5 * 1024 * 1024)
            return (false, "El archivo supera el límite de 5 MB.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

        var mimePermitidos = new Dictionary<string, string>
        {
            [".pdf"] = "application/pdf",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png"
        };

        if (!mimePermitidos.TryGetValue(extension, out var mime) ||
            !string.Equals(archivo.ContentType, mime,
                StringComparison.OrdinalIgnoreCase))
            return (false, "Solo se permiten archivos PDF, JPG o PNG.");

        var idEstado = await _repository.ObtenerEstadoActivoAsync();

        if (idEstado is null)
            return (false, "No hay un estado de documento activo configurado.");

        var nombreSeguro = Path.GetFileName(archivo.FileName);
        var nombreObjeto = $"{Guid.NewGuid()}_{nombreSeguro}";
        var ruta = $"{authUserId}/{idVehiculo}/{nombreObjeto}";

        var respuestaStorage = await SubirStorageAsync(
            ruta, archivo, mime, token);

        if (!respuestaStorage.IsSuccessStatusCode)
            return (false, "No se pudo guardar el archivo en Supabase Storage.");

        try
        {
            var documento = new DocumentosVehiculo
            {
                IdVehiculo = idVehiculo,
                IdTipoDocumento = idTipoDocumento,
                IdEstadoDocumento = idEstado.Value,
                NombreArchivo = nombreSeguro,
                UrlArchivo = ruta,
                FechaRegistro = DateTime.UtcNow,
                FechaVencimiento = fechaVencimiento.HasValue
                    ? DateOnly.FromDateTime(fechaVencimiento.Value)
                    : null
            };

            await _repository.AgregarAsync(documento);

            return (true, "Documento guardado correctamente.");
        }
        catch
        {
            await EliminarStorageAsync(ruta, token);
            throw;
        }
    }

    public async Task<(bool Exito, string Mensaje)> EliminarAsync(
        long idDocumento, long idUsuario, string token)
    {
        var documento = await _repository.ObtenerDocumentoDeUsuarioAsync(
            idDocumento, idUsuario);

        if (documento is null)
            return (false, "No se encontró el documento.");

        var ruta = documento.UrlArchivo;

        if (!string.IsNullOrWhiteSpace(ruta))
        {
            var respuesta = await EliminarStorageAsync(ruta, token);

            if (!respuesta.IsSuccessStatusCode &&
                respuesta.StatusCode != System.Net.HttpStatusCode.NotFound)
                return (false, "No se pudo eliminar el archivo de Storage.");
        }

        await _repository.EliminarAsync(documento);

        return (true, "Documento eliminado correctamente.");
    }

    private async Task<HttpResponseMessage> SubirStorageAsync(
        string ruta,
        IFormFile archivo,
        string mime,
        string token)
    {
        var client = CrearClienteStorage(token);

        var url = $"{ObtenerUrl()}/storage/v1/object/{Bucket}/{CodificarRuta(ruta)}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-upsert", "false");

        var stream = archivo.OpenReadStream();
        var contenido = new StreamContent(stream);
        contenido.Headers.ContentType = new MediaTypeHeaderValue(mime);
        request.Content = contenido;

        return await client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> EliminarStorageAsync(
        string ruta, string token)
    {
        var client = CrearClienteStorage(token);

        var url = $"{ObtenerUrl()}/storage/v1/object/{Bucket}";

        var body = JsonSerializer.Serialize(new
        {
            prefixes = new[] { ruta }
        });

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Content = new StringContent(
            body, Encoding.UTF8, "application/json");

        return await client.SendAsync(request);
    }

    private HttpClient CrearClienteStorage(string token)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add(
            "apikey", _configuration["Supabase:AnonKey"]);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private string ObtenerUrl()
        => _configuration["Supabase:Url"]!.TrimEnd('/');

    private static string CodificarRuta(string ruta)
        => string.Join("/", ruta.Split('/').Select(Uri.EscapeDataString));
}