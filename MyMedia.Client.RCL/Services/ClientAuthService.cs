using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization; // ⚠️ Importante para o AuthenticationStateProvider
using MyMedia.Client.RCL.Models;

namespace MyMedia.Client.RCL.Services
{
    public class ClientAuthService
    {
        private readonly HttpClient _http;
        // Injetamos a classe genérica para evitar erros de "Service Not Registered"
        private readonly AuthenticationStateProvider _authStateProvider;

        public ClientAuthService(HttpClient http, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _authStateProvider = authStateProvider;
        }

        // --- FUNÇÃO DE REGISTO ---
        public async Task<bool> RegisterAsync(RegisterDto model)
        {
            var result = await _http.PostAsJsonAsync("api/auth/register", model);
            return result.IsSuccessStatusCode;
        }

        // --- FUNÇÃO DE LOGIN ---
        public async Task<bool> LoginAsync(LoginDto model)
        {
            try
            {
                var result = await _http.PostAsJsonAsync("api/auth/login", model);

                if (result.IsSuccessStatusCode)
                {
                    // 1. CORREÇÃO: Temos de ler os dados que a API devolveu!
                    // O CustomAuthStateProvider precisa de saber o Nome/ID do user.
                    var userDto = await result.Content.ReadFromJsonAsync<LoginResponseDto>();

                    // 2. CORREÇÃO: Convertemos o provider e passamos os dados
                    if (_authStateProvider is CustomAuthStateProvider customProvider)
                    {
                        // Agora passamos o 'userDto' para dentro da função!
                        customProvider.NotifyLogin(userDto);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro Login: {ex.Message}");
            }

            return false;
        }

        // --- FUNÇÃO DE LOGOUT ---
        public async Task LogoutAsync()
        {
            // Avisa a API para limpar o Cookie (importante para a Web)
            await _http.PostAsync("api/auth/logout", null);

            // Avisa a App para limpar a Memória (importante para o Mobile)
            if (_authStateProvider is CustomAuthStateProvider customProvider)
            {
                // CORREÇÃO: Usamos NotifyLogout() e não NotifyLogin()
                customProvider.NotifyLogout();
            }
        }
    }
}