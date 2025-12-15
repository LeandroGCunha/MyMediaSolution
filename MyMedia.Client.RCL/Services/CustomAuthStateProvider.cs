using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace MyMedia.Client.RCL.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;

        public CustomAuthStateProvider(HttpClient http)
        {
            _http = http;
        }

        // Esta função corre sempre que a página carrega para ver se o user está logado
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Pergunta à API: "Quem sou eu?"
                var userInfo = await _http.GetFromJsonAsync<UserInfo>("api/auth/user");

                if (userInfo != null && userInfo.IsAuthenticated)
                {
                    // Se a API responder que sim, cria a "identidade" no Blazor
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userInfo.UserName)
                    };

                    var identity = new ClaimsIdentity(claims, "CookieAuth");
                    var user = new ClaimsPrincipal(identity);

                    return new AuthenticationState(user);
                }
            }
            catch
            {
                // Se der erro ou não estiver logado, ignora
            }

            // Se ninguém estiver logado, retorna vazio (Anónimo)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Chamamos isto no Login.razor para forçar a atualização do menu
        public void NotifyLogin()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    // Uma classe pequena só para ler a resposta da API
    public class UserInfo
    {
        public string UserName { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
    }
}