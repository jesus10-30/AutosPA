using System.Net;
using System.Text.Json.Serialization;

namespace AutosPAService.Services
{
    public class SupabaseAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _anonKey;

        public SupabaseAuthService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;

            _supabaseUrl = configuration["Supabase:Url"]?.TrimEnd('/')
                ?? throw new InvalidOperationException(
                    "Falta configurar Supabase:Url.");

            _anonKey = configuration["Supabase:AnonKey"]
                ?? throw new InvalidOperationException(
                    "Falta configurar Supabase:AnonKey.");
        }

        public async Task<bool> CambiarPasswordAsync(
            string email,
            string passwordActual,
            string passwordNueva,
            string authUserIdEsperado)
        {
            // 1. Verificar la contraseña actual.
            var autenticacion = await IniciarSesionAsync(
                email,
                passwordActual);

            if (autenticacion is null)
                return false;

            // 2. Comprobar que las credenciales corresponden al usuario esperado.
            if (!Guid.TryParse(autenticacion.UserId, out var authUserId) ||
                !Guid.TryParse(authUserIdEsperado, out var esperado) ||
                authUserId != esperado)
            {
                return false;
            }

            // 3. Actualizar la contraseña en Supabase Auth.
            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"{_supabaseUrl}/auth/v1/user");

            request.Headers.Add("apikey", _anonKey);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    autenticacion.AccessToken);

            request.Content = JsonContent.Create(new
            {
                password = passwordNueva
            });

            using var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<SupabaseAuthResult?> IniciarSesionAsync(
            string email,
            string password)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_supabaseUrl}/auth/v1/token?grant_type=password");

            request.Headers.Add("apikey", _anonKey);

            request.Content = JsonContent.Create(new
            {
                email,
                password
            });

            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var resultado =
                await response.Content.ReadFromJsonAsync<SupabaseTokenResponse>();

            if (resultado?.User?.Id is null ||
                string.IsNullOrWhiteSpace(resultado.AccessToken))
            {
                return null;
            }

            return new SupabaseAuthResult
            {
                UserId = resultado.User.Id,
                AccessToken = resultado.AccessToken,
                RefreshToken = resultado.RefreshToken ?? string.Empty,
                ExpiresIn = resultado.ExpiresIn
            };
        }

        public async Task<string?> RegistrarUsuarioAsync(
            string email,
            string password)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_supabaseUrl}/auth/v1/signup");

            request.Headers.Add("apikey", _anonKey);

            request.Content = JsonContent.Create(new
            {
                email,
                password
            });

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.BadRequest ||
                    response.StatusCode == HttpStatusCode.Conflict)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();
            }

            using var documento = System.Text.Json.JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

            var raiz = documento.RootElement;

            string? userId = null;

            if (raiz.TryGetProperty("id", out var id))
            {
                userId = id.GetString();
            }
            else if (raiz.TryGetProperty("user", out var user) &&
                     user.ValueKind == System.Text.Json.JsonValueKind.Object &&
                     user.TryGetProperty("id", out var nestedId))
            {
                userId = nestedId.GetString();
            }

            return Guid.TryParse(userId, out var authUserId)
                ? authUserId.ToString()
                : null;
        }

        private sealed class SupabaseTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonPropertyName("refresh_token")]
            public string? RefreshToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("user")]
            public SupabaseUser? User { get; set; }
        }

        private sealed class SupabaseUser
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }
        }
    }
    public class SupabaseAuthResult
    {
        public string UserId { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }
    }
}
