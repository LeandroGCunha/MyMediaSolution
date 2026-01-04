using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Configuration; // ⚠️ Necessário para a config
using MyMedia.Client.RCL.Services; // ⚠️ Onde está o CustomAuthStateProvider

namespace MyMedia.Client.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // ============================================================
            // 1. CONFIGURAÇÃO "IS MOBILE" (Para funcionar no Windows)
            // ============================================================
            // Isto diz ao Login.razor: "Sim, sou uma app mobile" (mesmo no Windows)
            var settings = new Dictionary<string, string> { { "IsMobile", "true" } };
            builder.Configuration.AddInMemoryCollection(settings);


            // ============================================================
            // 2. HTTP CLIENT (COM BYPASS DE SSL PARA ANDROID)
            // ============================================================

            builder.Services.AddScoped(sp =>
            {
                // ⚠️ PASSO CRÍTICO:
                // 1. Vai à janela "Output" ou ao topo do Visual Studio (onde diz o nome do túnel).
                // 2. Copia o endereço (ex: https://leandro-app-7198.devtunnels.ms/)
                // 3. Cola AQUI em baixo 👇

                string devTunnelUrl = "https://csqnv48d-7198.uks1.devtunnels.ms";

                // Nota: Com o Dev Tunnel, já não precisas do "if Android 10.0.2.2".
                // O link é público e funciona igual no Windows e no Android.

                // Handler para ignorar erros SSL (necessário mesmo com túneis às vezes)
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true;

                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(devTunnelUrl)
                };
            });


            // ============================================================
            // 3. AUTENTICAÇÃO (O CORRETO)
            // ============================================================
            builder.Services.AddAuthorizationCore();

            // ⚠️ MUDANÇA IMPORTANTE: 
            // Usamos o CustomAuthStateProvider da RCL (que tem o NotifyLogin)
            // e NÃO o AuthStateProviderMobile vazio.
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            // Registar o Serviço de Auth (para o Login funcionar)
            builder.Services.AddScoped<ClientAuthService>();


            // ============================================================
            // 4. OUTROS SERVIÇOS
            // ============================================================
            builder.Services.AddScoped<CarrinhoService>();
            builder.Services.AddScoped<ClientProdutoService>();

            return builder.Build();
        }
    }
}