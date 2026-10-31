
using AutosPALibrary.Dtos;
using AutosPAService.Entities;
using AutosPAService.Repositories;
using AutosPAService.Repositories.Registro;
using AutosPAService.Services.Registro;

namespace AutosPAService.Services;

public class RegistroService : IRegistroService
{
    private readonly SupabaseAuthService _supabaseAuthService;
    private readonly IRegistroRepository _registroRepository;

    public RegistroService(
        SupabaseAuthService supabaseAuthService,
        IRegistroRepository registroRepository)
    {
        _supabaseAuthService = supabaseAuthService;
        _registroRepository = registroRepository;
    }

    public async Task<bool> RegistrarAsync(RegistroRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // 1. Comprobar si el correo ya existe en PostgreSQL.
        var existente =
            await _registroRepository.ObtenerPorEmailAsync(email);

        if (existente is not null)
            return false;

        if (!request.OmitirVehiculo)
        {
            if (request.IdMarca is null || request.IdMarca <= 0)
                throw new ArgumentException("Selecciona una marca válida.");

            if (request.IdTipoVehiculo is null || request.IdTipoVehiculo <= 0)
                throw new ArgumentException("Selecciona un tipo de vehículo válido.");
        }
        // 2. Crear la cuenta en Supabase Auth.
        var authUserIdTexto =
            await _supabaseAuthService.RegistrarUsuarioAsync(
                email,
                request.Password);

        if (!Guid.TryParse(authUserIdTexto, out var authUserId))
            return false;

        // 3. Crear el usuario de FlexiCar.
        var usuario = new Usuario
        {
            IdRol = 1, // CLIENTE
            IdEstadoUsuario = 1, // ACTIVO
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Email = email,
            Telefono = string.IsNullOrWhiteSpace(request.Telefono)
                ? null
                : request.Telefono.Trim(),
            AuthUserId = authUserId,
            FechaRegistro = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        await _registroRepository.CrearUsuarioAsync(usuario);

        // 4. Si no registrará vehículo, terminamos.
        if (request.OmitirVehiculo)
            return true;

        // 5. Crear al usuario como conductor.
        var conductor = new Conductore
        {
            IdUsuario = usuario.IdUsuario,
            NumeroLicencia = request.NumeroLicencia.Trim(),
            FechaVencimientoLicencia =
                request.FechaVencimientoLicencia,
            FechaRegistro = DateTime.UtcNow
        };

        await _registroRepository.CrearConductorAsync(conductor);

        // 6. Crear el vehículo asociado al conductor.
        var vehiculo = new Vehiculo
        {
            IdConductor = conductor.IdConductor,
            IdMarca = request.IdMarca!.Value,
            IdTipoVehiculo = request.IdTipoVehiculo!.Value,
            Modelo = request.Modelo!.Trim(),
            Anio = request.Anio!.Value,
            Placa = request.Placa!.Trim().ToUpperInvariant(),
            Color = string.IsNullOrWhiteSpace(request.Color)
                ? null
                : request.Color.Trim(),
            Kilometraje = 0,
            FechaRegistro = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        await _registroRepository.CrearVehiculoAsync(vehiculo);

        return true;
    }

    
}
