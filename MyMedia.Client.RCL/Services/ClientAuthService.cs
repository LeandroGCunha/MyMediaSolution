using System.Net.Http.Json;
using MyMedia.Client.RCL.Models; // Onde puseste o RegisterDto

namespace MyMedia.Client.RCL.Services
{
    public class ClientAuthService
    {
        private readonly HttpClient _http;
        private readonly CustomAuthStateProvider _authStateProvider;

        // Injetamos o HttpClient e o TEU CustomAuthStateProvider
        public ClientAuthService(HttpClient http, CustomAuthStateProvider authStateProvider)
        {
            _http = http;
            _authStateProvider = authStateProvider;
        }

        // --- FUNÇÃO DE REGISTO ---
        public async Task<bool> RegisterAsync(RegisterDto model)
        {
            // Envia os dados para a API (AuthController)
            var result = await _http.PostAsJsonAsync("api/auth/register", model);

            // Se devolver 200 OK, correu bem
            return result.IsSuccessStatusCode;
        }

        // --- FUNÇÃO DE LOGIN ---
        public async Task<bool> LoginAsync(LoginDto model)
        {
            var result = await _http.PostAsJsonAsync("api/auth/login", model);

            if (result.IsSuccessStatusCode)
            {
                // SEGREDO: Avisar o site que o user entrou!
                _authStateProvider.NotifyLogin();
                return true;
            }

            return false;
        }

        // --- FUNÇÃO DE LOGOUT ---
        public async Task LogoutAsync()
        {
            await _http.PostAsync("api/auth/logout", null);
            _authStateProvider.NotifyLogin(); // Avisa que saiu (para limpar o menu)
        }
    }
}