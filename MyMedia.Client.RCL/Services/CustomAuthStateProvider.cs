using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MyMedia.Client.RCL.Models; // ⚠️ OBRIGATÓRIO: Garante que tens este using!

namespace MyMedia.Client.RCL.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;

        // Esta variável serve para guardar o login no Mobile (Memória RAM)
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(HttpClient http)
        {
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 1. (MOBILE) Se já temos o user guardado na memória, usa-o logo!
            if (_currentUser.Identity.IsAuthenticated)
            {
                return new AuthenticationState(_currentUser);
            }

            // 2. (WEB) Se não temos na memória, pergunta à API (verifica Cookies)
            try
            {
                var userInfo = await _http.GetFromJsonAsync<UserInfo>("api/auth/user");

                if (userInfo != null && userInfo.IsAuthenticated)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userInfo.UserName)
                    };

                    var identity = new ClaimsIdentity(claims, "CookieAuth");
                    _currentUser = new ClaimsPrincipal(identity); // Guarda para não perguntar sempre
                }
            }
            catch
            {
                // Ignora erros (simplesmente não está logado)
            }

            // Retorna o user (seja ele Autenticado ou Anónimo)
            return new AuthenticationState(_currentUser);
        }

        // =========================================================
        // AQUI ESTÁ A CORREÇÃO QUE PRECISAS
        // =========================================================

        // Agora aceita o 'userDto' para o Mobile funcionar
        public void NotifyLogin(LoginResponseDto userDto)
        {
            var claims = new List<Claim>
            {
        
                new Claim(ClaimTypes.NameIdentifier, userDto.UserId ?? "")
            };

            // Adiciona as Roles (Admin, Cliente, etc)
            if (userDto.Roles != null)
            {
                foreach (var role in userDto.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // Cria a identidade e guarda na memória
            var identity = new ClaimsIdentity(claims, "MobileAuth");
            _currentUser = new ClaimsPrincipal(identity);

            // Avisa todos os componentes que o user mudou!
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        // Método para fazer Logout manual no Mobile
        public void NotifyLogout()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    // Classe auxiliar para a resposta da API (Web)
    public class UserInfo
    {
        public string UserName { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
    }
}