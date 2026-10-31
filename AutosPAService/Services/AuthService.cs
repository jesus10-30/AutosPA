using AutosPALibrary.Dtos;
using AutosPAService.Repositories;
namespace AutosPAService.Services
{
    public class AuthService
    {
        private readonly SupabaseAuthService _supabaseAuthService;
        private readonly AuthRepository _authRepository;

        public AuthService(
            SupabaseAuthService supabaseAuthService,
            AuthRepository authRepository)
        {
            _supabaseAuthService = supabaseAuthService;
            _authRepository = authRepository;
        }

        public async Task<LoginResponse?> IniciarSesionAsync(
            LoginRequest request)
        {
            var autenticacion =
                await _supabaseAuthService.IniciarSesionAsync(
                    request.Email.Trim(),
                    request.Password);

            if (autenticacion is null ||
                !Guid.TryParse(autenticacion.UserId, out var authUserId))
            {
                return null;
            }

            var usuario = await _authRepository
                .ObtenerPorAuthUserIdAsync(authUserId);

            if (usuario is null)
            {
                return null;
            }

            var estado = usuario.IdEstadoUsuarioNavigation;
            var rol = usuario.IdRolNavigation;

            if (!estado.Activo ||
                !string.Equals(
                    estado.Nombre,
                    "ACTIVO",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!rol.Activo)
            {
                return null;
            }

            return new LoginResponse
            {
                IdUsuario = usuario.IdUsuario,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Email = usuario.Email,
                Rol = rol.Nombre,
                AccessToken = autenticacion.AccessToken,
                RefreshToken = autenticacion.RefreshToken,
                ExpiresIn = autenticacion.ExpiresIn
            };
        }

        public async Task<PerfilUsuarioDto?> ObtenerPerfilAsync(long idUsuario)
        {
            var usuario = await _authRepository.ObtenerPorIdAsync(idUsuario);

            if (usuario is null)
                return null;

            return new PerfilUsuarioDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                FechaRegistro = usuario.FechaRegistro,
                Rol = usuario.IdRolNavigation.Nombre
            };
        }

        public async Task<bool> ActualizarPerfilAsync(
            long idUsuario,
            ActualizarPerfilRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombres))
                throw new ArgumentException("Los nombres son obligatorios.");

            if (string.IsNullOrWhiteSpace(request.Apellidos))
                throw new ArgumentException("Los apellidos son obligatorios.");

            return await _authRepository.ActualizarPerfilAsync(
                idUsuario,
                request.Nombres.Trim(),
                request.Apellidos.Trim(),
                string.IsNullOrWhiteSpace(request.Telefono)
                    ? null
                    : request.Telefono.Trim());
        }


        public async Task<bool> CambiarPasswordAsync(
            CambiarPasswordRequest request)
        {
            if (request.IdUsuario <= 0)
                throw new ArgumentException("El usuario no es válido.");

            if (string.IsNullOrWhiteSpace(request.PasswordActual))
                throw new ArgumentException("La contraseña actual es obligatoria.");

            if (string.IsNullOrWhiteSpace(request.PasswordNueva) ||
                request.PasswordNueva.Length < 8)
            {
                throw new ArgumentException(
                    "La nueva contraseña debe tener al menos 8 caracteres.");
            }

            if (request.PasswordActual == request.PasswordNueva)
            {
                throw new ArgumentException(
                    "La nueva contraseña debe ser diferente de la actual.");
            }

            var usuario = await _authRepository
                .ObtenerUsuarioParaCambiarPasswordAsync(request.IdUsuario);

            if (usuario is null ||
                usuario.AuthUserId is null ||
                string.IsNullOrWhiteSpace(usuario.Email))
            {
                return false;
            }

            return await _supabaseAuthService.CambiarPasswordAsync(
                usuario.Email,
                request.PasswordActual,
                request.PasswordNueva,
                usuario.AuthUserId.Value.ToString());
        }


        public async Task<EstadisticasCuentaDto?> ObtenerEstadisticasCuentaAsync(
            long idUsuario)
        {
            return await _authRepository.ObtenerEstadisticasCuentaAsync(idUsuario);
        }

    }
}
